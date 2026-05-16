using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TECHNICAL_SERVICE_REQUEST.Controllers;
using TECHNICAL_SERVICE_REQUEST.Core;
using TECHNICAL_SERVICE_REQUEST.Enumerables;
using TECHNICAL_SERVICE_REQUEST.Models;
using TECHNICAL_SERVICE_REQUEST.Services;

namespace TECHNICAL_SERVICE_REQUEST.Core
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
}