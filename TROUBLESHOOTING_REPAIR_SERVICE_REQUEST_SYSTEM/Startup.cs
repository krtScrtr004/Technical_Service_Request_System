using Microsoft.Owin;
using Owin;
using Quartz;
using Quartz.Impl;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Job;

[assembly: OwinStartupAttribute(typeof(TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Startup))]
namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            // Map SignalR hubs
            app.MapSignalR();

            // Schedule the job to delete old notifications daily at 2:00 AM
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;
            scheduler.Start();

            // Schedule the job to assign queued requests daily at 1:00 AM
            var assignedQueuedRequestJob = JobBuilder.Create<AssignedQueuedRequestJob>().Build();
            var assignedQueuedRequestTrigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule(s =>
                    s.OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(1, 0))) // Schedule for 1:00 AM daily
                    .Build();
            scheduler.ScheduleJob(assignedQueuedRequestJob, assignedQueuedRequestTrigger);

            // Schedule the job to delete old notifications daily at 2:00 AM
            var deleteOldNotificationJob = JobBuilder.Create<DeleteOldNotificationsJob>().Build();
            var deleteOldNotificationTrigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule(s =>
                    s.OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(2, 0))) // Schedule for 2:00 AM daily
                .Build();
            scheduler.ScheduleJob(deleteOldNotificationJob, deleteOldNotificationTrigger);
        }
    }
}
