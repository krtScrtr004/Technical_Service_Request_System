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

            IJobDetail job = JobBuilder.Create<DeleteOldNotificationsJob>().Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule(s =>
                    s.OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(2, 0))) // Schedule for 2:00 AM daily
                .Build();

            scheduler.ScheduleJob(job, trigger);
        }
    }
}
