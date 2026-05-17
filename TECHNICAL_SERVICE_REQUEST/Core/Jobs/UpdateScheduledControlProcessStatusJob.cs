using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TECHNICAL_SERVICE_REQUEST.Core;
using TECHNICAL_SERVICE_REQUEST.Enumerables;
using TECHNICAL_SERVICE_REQUEST.Models;
using TECHNICAL_SERVICE_REQUEST.Services;

namespace TECHNICAL_SERVICE_REQUEST.Core
{
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
                    RecipientId = clientId,
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
                    RecipientId = technicianId,
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
}