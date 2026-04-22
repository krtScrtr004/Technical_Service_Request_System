using Microsoft.Owin;
using Owin;
using Quartz;
using Quartz.Impl;
using Serilog;
using System;
using System.Web.Hosting;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Job;

[assembly: OwinStartupAttribute(typeof(TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Startup))]
namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            try
            {
                Log.Information("Application Starting Up");
                InitialiazeLogger();

                ConfigureAuth(app);

                // Map SignalR hubs
                app.MapSignalR();
                Log.Information("SignalR hubs mapped successfully.");

                InitializeScheduledJobs();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while starting the scheduler.");
            }
        }

        private void InitialiazeLogger()
        {
            var logDirectory = HostingEnvironment.MapPath("~/Logs") ?? "Logs";
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug() // Set minimum log level
               .WriteTo.File(
                    $"{logDirectory}/info-.txt",
                    rollingInterval: RollingInterval.Day, // Create new file daily
                    retainedFileCountLimit: 30, // Retain last 30 (days) logs
                    fileSizeLimitBytes: 100_000_000, // Limit log file to 100MB
                    rollOnFileSizeLimit: true, // Create new file when the size limit is reached
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
               .WriteTo.File(
                    $"{logDirectory}/warning-.txt",
                    rollingInterval: RollingInterval.Day, // Create new file daily
                    retainedFileCountLimit: 30, // Retain last 30 (days) logs
                    fileSizeLimitBytes: 100_000_000, // Limit log file to 100MB
                    rollOnFileSizeLimit: true, // Create new file when the size limit is reached
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
               .WriteTo.File(
                    $"{logDirectory}/error.txt",
                    rollingInterval: RollingInterval.Day, // Create new file daily
                    retainedFileCountLimit: 30, // Retain last 30 (days) logs
                    fileSizeLimitBytes: 100_000_000, // Limit log file to 100MB
                    rollOnFileSizeLimit: true, // Create new file when the size limit is reached
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
               .CreateLogger();
        }

        private void InitializeScheduledJobs()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;
            scheduler.Start();

            // Schedule the job to update scheduled control process status every hour from 8:00 AM to 4:00 PM
            var updateScheduledControlProcessStatusJob = JobBuilder.Create<UpdateScheduledControlProcessStatusJob>().Build();
            var updateScheduledControlProcessStatusJobTrigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule(s =>
                    s.OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(8, 0)) // Start at 8:00 AM
                    .EndingDailyAt(TimeOfDay.HourAndMinuteOfDay(16, 0)) // End at 4:00 PM
                    .WithIntervalInHours(1)) // Run every hour
                .Build();
            scheduler.ScheduleJob(updateScheduledControlProcessStatusJob, updateScheduledControlProcessStatusJobTrigger);

            // Schedule the job to assign queued requests daily at 1:00 AM
            var assignQueuedRequestJob = JobBuilder.Create<AssignQueuedRequestJob>().Build();
            var assignQueuedRequestTrigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule(s =>
                    s.OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(1, 0)))
                .Build();
            scheduler.ScheduleJob(assignQueuedRequestJob, assignQueuedRequestTrigger);

            // Schedule the job to delete old notifications daily at 2:30 AM
            var deleteOldNotificationJob = JobBuilder.Create<DeleteOldNotificationsJob>().Build();
            var deleteOldNotificationTrigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule(s =>
                    s.OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(2, 30)))
                .Build();
            scheduler.ScheduleJob(deleteOldNotificationJob, deleteOldNotificationTrigger);

            // Schedule the job to deactivate expired registrations daily at 3:30 AM
            var deactivateExpiredRegistrationsJob = JobBuilder.Create<DeactivateExpiredRegistrationsJob>().Build();
            var deactivateExpiredRegistrationsTrigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule(s =>
                    s.OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(3, 30)))
                .Build();
            scheduler.ScheduleJob(deactivateExpiredRegistrationsJob, deactivateExpiredRegistrationsTrigger);

            Log.Information("Scheduler started successfully.");
        }

    }

}
