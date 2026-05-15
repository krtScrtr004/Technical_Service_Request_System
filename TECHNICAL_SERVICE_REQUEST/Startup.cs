using Microsoft.Owin;
using Owin;
using Quartz;
using Quartz.Impl;
using Serilog;
using System;
using System.Web.Hosting;
using TECHNICAL_SERVICE_REQUEST.Job;
using System.Configuration;

[assembly: OwinStartupAttribute(typeof(TECHNICAL_SERVICE_REQUEST.Startup))]
namespace TECHNICAL_SERVICE_REQUEST
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

            // Every day from 7:00 AM to 4:00 PM
            var notifyTechnicianUpcomingServiceJob = JobBuilder.Create<NotifyTechnicianUpcomingServiceJob>().Build();
            var notifyTechnicianUpcomingServiceTrigger = TriggerBuilder.Create()
                .WithCronSchedule("0 0 7-16 * * ?")
                .Build();
            scheduler.ScheduleJob(notifyTechnicianUpcomingServiceJob, notifyTechnicianUpcomingServiceTrigger);

            // Every hour from 8:00 AM to 4:00 PM
            var updateScheduledControlProcessStatusJob = JobBuilder.Create<UpdateScheduledControlProcessStatusJob>().Build();
            var updateScheduledControlProcessStatusJobTrigger = TriggerBuilder.Create()
                .WithCronSchedule("0 0 8-16 * * ?")
                .Build();
            scheduler.ScheduleJob(updateScheduledControlProcessStatusJob, updateScheduledControlProcessStatusJobTrigger);

            // Once daily at 1:00 AM
            var assignQueuedRequestJob = JobBuilder.Create<AssignQueuedRequestJob>().Build();
            var assignQueuedRequestTrigger = TriggerBuilder.Create()
                .WithCronSchedule("0 0 1 * * ?")
                .Build();
            scheduler.ScheduleJob(assignQueuedRequestJob, assignQueuedRequestTrigger);

            // Once daily at 2:30 AM
            var deleteOldNotificationJob = JobBuilder.Create<DeleteOldNotificationsJob>().Build();
            var deleteOldNotificationTrigger = TriggerBuilder.Create()
                .WithCronSchedule("0 30 2 * * ?")
                .Build();
            scheduler.ScheduleJob(deleteOldNotificationJob, deleteOldNotificationTrigger);

            // Once daily at 3:30 AM
            var deactivateExpiredAppUsersJob = JobBuilder.Create<DeactivateExpiredAppUsersJob>().Build();
            var deactivateExpiredAppUsersTrigger = TriggerBuilder.Create()
                .WithCronSchedule("0 30 3 * * ?")
                .Build();
            scheduler.ScheduleJob(deactivateExpiredAppUsersJob, deactivateExpiredAppUsersTrigger);
            Log.Information("Scheduler started successfully.");
        }

    }

}
