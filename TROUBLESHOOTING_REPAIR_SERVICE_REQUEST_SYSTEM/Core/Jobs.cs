using Quartz;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Data.Entity;
using System.Web;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Enumerables;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Controllers;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Services;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Job
{
    public class AssignedQueuedRequestJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var controller = new TechnicalServiceRequestsController();
            var notificationService = new NotificationService();


            using (var _db = new ApplicationDbContext())
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        var queuedRequests = _db.TechnicalServiceRequestQueues
                            .Include(r => r.TechnicalServiceRequest)
                            .Where(r => !r.IsProcessed)
                            .OrderBy(r => r.QueuedAt)
                            .Select(r => r.TechnicalServiceRequest)
                            .ToList();

                        // Process each queued request
                        foreach (var request in queuedRequests)
                        {
                            var requestTypeId = request.TechnicalServiceTypeId;
                            if (!requestTypeId.HasValue)
                            {
                                continue;
                            }

                            var clientId = _db.Registrations
                                .Where(r => r.Email == request.ClientEmailAddress)
                                .Select(r => r.Id)
                                .FirstOrDefault();

                            var availableTechnicianId = controller.GetAvailableTechnician();
                            if (availableTechnicianId == null)
                            {
                                // No technician is available, means no available technician for the succeeding requests
                                break;
                            }

                            // Create new history for the request
                            request.TechnicalServiceRequestHistories = new List<TechnicalServiceRequestHistory>
                            {
                                new TechnicalServiceRequestHistory
                                {
                                    ActionTakenByRegistrationId = availableTechnicianId,
                                    TechnicalServiceRequestStatusId = (int)TechnicalServiceRequestStatusEnum.ONGOING,
                                    DateAction = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                }
                            };

                            request.DateReceived = DateTime.Now;
                            request.TechnicalServiceRequestStatusId =
                                (int)TechnicalServiceRequestStatusEnum.ONGOING;

                            _db.Entry(request).State = EntityState.Modified;

                            // Notify the technician for the assignment
                            notificationService.NotifyTechnicianAssignment(
                                availableTechnicianId.Value,
                                request.ReferenceCode
                            );

                            // Notify the client
                            if (clientId > 0)
                            {
                                notificationService.RefreshUserUi(clientId);
                            }
                            TechnicalServiceRequestHub.RefreshTechnicalServiceRequestList();
                        }

                        _db.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                    }
                }
            }
            return Task.CompletedTask;
        }
    }

    // Delete notifications older than 30 days
    public class DeleteOldNotificationsJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
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
            }
            return Task.CompletedTask;
        }

    }
}