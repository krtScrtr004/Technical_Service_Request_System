using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Core;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Controllers;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Enumerables;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Services;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Job
{
    [DisallowConcurrentExecution]
    public class AssignQueuedRequestJob : IJob
    {
        // Assign queued technical service requests to available technicians
        public Task Execute(IJobExecutionContext context)
        {
            Log.Information($"AssignedQueuedRequestJob started at {DateTime.Now}.");

            var controller = new RequestController();
            var notificationService = new NotificationService();

            var assignedRequests = 0;

            using (var _db = new ApplicationDbContext())
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        var queuedRequests = _db.RequestQueues
                            .Include(r => r.Request)
                            .Where(r =>
                                !r.IsProcessed &&
                                 r.Request.StatusId == (int)RequestStatusEnum.PENDING)
                            .OrderBy(r => r.QueuedAt)
                            .Select(r => r.Request)
                            .ToList();

                        // Process each queued request
                        foreach (var request in queuedRequests)
                        {
                            var availableTechnicianId = controller.GetAvailableTechnician();
                            if (availableTechnicianId == null)
                            {
                                // No technician is available, means no available technician for the succeeding requests
                                break;
                            }

                            // Create new history for the request
                            request.Histories = new List<RequestHistory>
                            {
                                new RequestHistory
                                {
                                    ActionTakenById = availableTechnicianId,
                                    StatusId = (int)RequestStatusEnum.ONGOING,
                                    DateAction = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                }
                            };

                            request.DateReceived = DateTime.Now;
                            request.StatusId = (int)RequestStatusEnum.ONGOING;

                            _db.Entry(request).State = EntityState.Modified;

                            // Notify the technician for the assignment
                            notificationService.NotifyTechnicianAssignment(
                                availableTechnicianId.Value,
                                request.ReferenceCode
                            );

                            // Notify the client
                            notificationService.RefreshUserUi(request.ClientId);
                            RequestHub.RefreshRequestList();

                            assignedRequests++;
                        }

                        _db.SaveChanges();
                        transaction.Commit();

                        Log.Information($"AssignedQueuedRequestJob executed successfully at {DateTime.Now}. Processed {queuedRequests.Count} queued requests - {assignedRequests} requests assigned.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Log.Error(ex, $"An error occurred while executing AssignedQueuedRequestJob at {DateTime.Now}.");
                    }
                }
            }
            return Task.CompletedTask;
        }
    }

    [DisallowConcurrentExecution]
    public class NotifyTechnicianUpcomingServiceJob : IJob
    {
        // Notify technicians about their upcoming scheduled services within the next hour
        public Task Execute(IJobExecutionContext context)
        {
            var notificationService = new NotificationService();

            using (var db = new ApplicationDbContext())
            {
                try
                {
                    var now = DateTime.Now;
                    var currentHour = TimeSpan.FromHours(now.Hour);
                    var oneHourLater = currentHour.Add(TimeSpan.FromHours(1));

                    var upcomingServices = db.Requests
                        .Include(r => r.Client)
                        .Include(r => r.ScheduledControlProcessDetail)
                        .Where(r =>
                            // Check if the request is pending
                            r.StatusId == (int)RequestStatusEnum.PENDING &&

                            r.ScheduledControlProcessDetailId.HasValue &&
                            r.ScheduledControlProcessDetail.ScheduledDate.HasValue &&
                            r.ScheduledControlProcessDetail.ScheduledStartTime.HasValue &&

                            // Check if the scheduled date is today and the scheduled start time is within the next hour
                            DbFunctions.TruncateTime(r.ScheduledControlProcessDetail.ScheduledDate.Value) == DbFunctions.TruncateTime(now) &&
                            r.ScheduledControlProcessDetail.ScheduledStartTime.Value >= currentHour &&
                            r.ScheduledControlProcessDetail.ScheduledStartTime.Value <= oneHourLater
                        )
                        .ToList();

                    foreach (var service in upcomingServices)
                    {
                        var latestHistory = db.RequestHistories
                            .Where(h => h.RequestId == service.Id)
                            .OrderByDescending(h => h.UpdatedAt)
                            .Select(h => new
                            {
                                h.ActionTakenById,
                                TechnicianFirstName = h.ActionTakenBy.FirstName
                            })
                            .FirstOrDefault();

                        var technicianId = latestHistory != null
                            ? latestHistory.ActionTakenById
                            : (int?)null;
                        if (technicianId == null)
                        {
                            continue;
                        }

                        // Notify the technician about the upcoming service
                        notificationService.NotifyTechnicianUpcomingService(
                            technicianId.Value,
                            service.ReferenceCode,
                            service.ScheduledControlProcessDetail.ScheduledStartTime.Value
                        );
                    }

                    Log.Information($"NotifyTechnicianUpcomingServiceJob executed successfully at {DateTime.Now}. Notified {upcomingServices.Count} technicians about upcoming services.");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"An error occurred while executing NotifyTechnicianUpcomingServiceJob at {DateTime.Now}.");
                }
            }

            return Task.CompletedTask;
        }
    }

    [DisallowConcurrentExecution]
    public class UpdateScheduledControlProcessStatusJob : IJob
    {
        // Update the status of scheduled control processes to ongoing when their scheduled start time has passed
        public Task Execute(IJobExecutionContext context)
        {
            Log.Information($"UpdateScheduledControlProcessStatusJob started at {DateTime.Now}.");

            var statusUpdates = new List<Tuple<int, int, string, string>>(); // clientId, technicianId, referenceCode, technicianFirstName
            var hasUpdates = false;

            using (var _db = new ApplicationDbContext())
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        var now = DateTime.Now;
                        var nowTime = now.TimeOfDay;

                        // Find all control processes that are scheduled for today and are still pending
                        var scheduledControlProcesses = _db.Requests
                            .Where(r =>
                                // Check if the request status is pending
                                r.StatusId == (int)RequestStatusEnum.PENDING &&
                                // Check if the request has a scheduled date and it is today
                                r.ScheduledControlProcessDetail.ScheduledDate.HasValue &&
                                DbFunctions.TruncateTime(now) == DbFunctions.TruncateTime(r.ScheduledControlProcessDetail.ScheduledDate.Value) &&
                                // Check if the request has a scheduled start time and it has passed
                                r.ScheduledControlProcessDetail.ScheduledStartTime.HasValue &&
                                nowTime >= r.ScheduledControlProcessDetail.ScheduledStartTime.Value &&
                                // Check if the request has a scheduled end time and it has not passed
                                r.ScheduledControlProcessDetail.ScheduledEndTime.HasValue &&
                                nowTime <= r.ScheduledControlProcessDetail.ScheduledEndTime.Value
                            )
                            .ToList();

                        // Update the status of each scheduled control process to ongoing and create a new history entry
                        foreach (var process in scheduledControlProcesses)
                        {
                            // Get the latest history entry for the process to determine the technician who will be taking the action
                            var latestHistory = _db.RequestHistories
                                .Where(h => h.RequestId == process.Id)
                                .OrderByDescending(h => h.UpdatedAt)
                                .Select(h => new
                                {
                                    h.ActionTakenById,
                                    TechnicianFirstName = h.ActionTakenBy.FirstName
                                })
                                .FirstOrDefault();

                            var technicianId = latestHistory != null
                                ? latestHistory.ActionTakenById
                                : null;
                            if (!technicianId.HasValue)
                            {
                                continue;
                            }

                            // Create new history entry for the status update
                            _db.RequestHistories.Add(new RequestHistory()
                            {
                                RequestId = process.Id,
                                StatusId = (int)RequestStatusEnum.ONGOING,
                                DateAction = now,
                                UpdatedAt = now,
                                ActionTaken = "Service has started.",
                                ActionTakenById = technicianId.Value,
                            });

                            process.StatusId = (int)RequestStatusEnum.ONGOING;
                            _db.Entry(process).State = EntityState.Modified;

                            statusUpdates.Add(Tuple.Create(
                                process.ClientId,
                                technicianId.Value,
                                process.ReferenceCode,
                                string.IsNullOrWhiteSpace(latestHistory.TechnicianFirstName) ? "Technician" : latestHistory.TechnicianFirstName
                            ));

                            hasUpdates = true;
                        }

                        _db.SaveChanges();
                        transaction.Commit();

                        Log.Information($"UpdateScheduledControlProcessStatusJob executed successfully at {DateTime.Now}. Updated {scheduledControlProcesses.Count} scheduled control processes.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Log.Error(ex, $"An error occurred while executing UpdateScheduledControlProcessStatusJob at {DateTime.Now}.");
                    }
                }

                // Send notifications to clients about the status updates
                if (hasUpdates)
                {
                    Log.Information($"Sending notifications for {statusUpdates.Count} status updates at {DateTime.Now}.");

                    var notificationService = new NotificationService();

                    // Notify each client about their respective request status update
                    foreach (var update in statusUpdates)
                    {
                        NotifyClient(update.Item1, update.Item3, update.Item4);
                        NotifyTechnician(update.Item2, update.Item3);
                    }

                    // Refresh the UI for each affected client and technician
                    foreach (var id in statusUpdates.Select(i => new { i.Item1, i.Item2 }).Distinct())
                    {
                        // Refresh client UI
                        notificationService.RefreshUserUi(id.Item1);
                        // Refresh technician UI
                        notificationService.RefreshUserUi(id.Item2);
                    }

                    RequestHub.RefreshRequestList();

                    Log.Information($"Sending notifications completed at {DateTime.Now}.");
                }
            }

            Log.Information($"UpdateScheduledControlProcessStatusJob completed at {DateTime.Now}.");
            return Task.CompletedTask;
        }

        private void NotifyClient(
            int clientId,
            string referenceCode,
            string technicianFirstName)
        {
            var notificationService = new NotificationService();
            var notificationMessage = notificationService.BuildRecipientMessageFromRequestStatus(
                (int)RequestStatusEnum.ONGOING,
                referenceCode,
                technicianFirstName
            );

            using (var db = new ApplicationDbContext())
            {
                db.Notifications.Add(new Notification()
                {
                    RecipientRegistrationId = clientId,
                    Title = "Technical Service Request Update",
                    Message = notificationMessage,
                    ForAdmin = false,
                    ForIT = false,
                    IsActive = true,
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                });
                db.SaveChanges();
            }

            notificationService.RefreshUserUi(clientId);
        }

        private void NotifyTechnician(int technicianId, string referenceCode)
        {
            var notificationService = new NotificationService();
            var notificationMessage = $"Service for request with reference code: {referenceCode} is now on going.";

            using (var db = new ApplicationDbContext())
            {
                db.Notifications.Add(new Notification()
                {
                    RecipientRegistrationId = technicianId,
                    Title = "Technical Service Request Update",
                    Message = notificationMessage,
                    ForAdmin = false,
                    ForIT = false,
                    IsActive = true,
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                });
                db.SaveChanges();
            }
            notificationService.RefreshUserUi(technicianId);
        }
    }


    [DisallowConcurrentExecution]
    public class DeleteOldNotificationsJob : IJob
    {
        // Delete notifications older than 30 days
        public Task Execute(IJobExecutionContext context)
        {
            Log.Information($"DeleteOldNotificationsJob started at {DateTime.Now}.");

            try
            {
                using (var _db = new ApplicationDbContext())
                {
                    var cutoff = DateTime.Now.AddDays(-30);
                    var oldNotifications = _db.Notifications.Where(n =>
                        n.CreatedAt < cutoff)
                        .ToList();
                    if (oldNotifications.Any())
                    {
                        _db.Notifications.RemoveRange(oldNotifications);
                        _db.SaveChanges();
                    }

                    Log.Information($"DeleteOldNotificationsJob executed successfully at {DateTime.Now}. Deleted {oldNotifications.Count} old notifications.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occurred while executing DeleteOldNotificationsJob at {DateTime.Now}.");
            }

            return Task.CompletedTask;
        }
    }

    public class DeactivateExpiredRegistrationsJob : IJob
    {
        // Deactivate registrations that have expired
        public Task Execute(IJobExecutionContext context)
        {
            Log.Information($"DeactivateExpiredRegistrationsJob started at {DateTime.Now}.");

            using (var db = new ApplicationDbContext())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {

                        var adminId = db.Registrations
                            .Where(r =>
                                r.IsActive &&
                                r.RoleId == AccountTypeEnum.ADMIN
                            )
                            .Select(r => r.Id)
                            .FirstOrDefault();
                        if (adminId < 1)
                        {
                            Log.Warning("No active admin found.");
                            return Task.CompletedTask;
                        }
                        Log.Information($"Active admin found with ID: {adminId}");

                        var expiredRegistrations = db.Registrations
                            .Where(r =>
                                r.IsActive &&
                                r.ExpiryDate.HasValue &&
                                DbFunctions.TruncateTime(r.ExpiryDate.Value) <= DbFunctions.TruncateTime(DateTime.Now)
                            )
                            .ToList();
                        foreach (var registration in expiredRegistrations)
                        {
                            registration.IsActive = false;
                            registration.DeactivatedByRegistrationId = adminId;
                            registration.DeactivatedRemarks = "Registration has expired";
                            db.Entry(registration).State = EntityState.Modified;
                        }

                        db.SaveChanges();
                        transaction.Commit();

                        Log.Information($"DeactivateExpiredRegistrationsJob executed successfully at {DateTime.Now}. Deactivated {expiredRegistrations.Count} expired registrations.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Log.Error(ex, $"An error occurred while executing DeactivateExpiredRegistrationsJob at {DateTime.Now}.");
                    }
                }
            }

            return Task.CompletedTask;
        }
    }

}