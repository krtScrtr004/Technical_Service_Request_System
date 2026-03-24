using Quartz;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Web;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Job
{
    public class DeleteOldNotificationsJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            using (var _db = new ApplicationDbContext())
            {
                // Delete notifications older than 30 days
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