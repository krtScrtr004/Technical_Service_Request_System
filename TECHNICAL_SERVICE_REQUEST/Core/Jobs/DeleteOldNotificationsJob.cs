using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TECHNICAL_SERVICE_REQUEST.Models;

namespace TECHNICAL_SERVICE_REQUEST.Core
{
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
}