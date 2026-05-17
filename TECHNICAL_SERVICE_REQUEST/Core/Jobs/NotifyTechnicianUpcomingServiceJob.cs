using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TECHNICAL_SERVICE_REQUEST.Enumerables;
using TECHNICAL_SERVICE_REQUEST.Models;
using TECHNICAL_SERVICE_REQUEST.Services;

namespace TECHNICAL_SERVICE_REQUEST.Core
{
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
}