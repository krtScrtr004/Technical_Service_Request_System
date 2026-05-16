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

namespace TECHNICAL_SERVICE_REQUEST.Jobs
{
    public class DeactivateExpiredAppUsersJob : IJob
    {
        // Deactivate registrations that have expired
        public Task Execute(IJobExecutionContext context)
        {
            Log.Information($"DeactivateExpiredAppUsersJob started at {DateTime.Now}.");

            using (var db = new ApplicationDbContext())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {

                        var adminId = db.AppUsers
                            .Where(r =>
                                r.IsActive &&
                                r.RoleId == AppUserRoleEnum.ADMIN
                            )
                            .Select(r => r.Id)
                            .FirstOrDefault();
                        if (adminId < 1)
                        {
                            Log.Warning("No active admin found.");
                            return Task.CompletedTask;
                        }
                        Log.Information($"Active admin found with ID: {adminId}");

                        var expiredAppUsers = db.AppUsers
                            .Where(r =>
                                r.IsActive &&
                                r.ExpiryDate.HasValue &&
                                DbFunctions.TruncateTime(r.ExpiryDate.Value) <= DbFunctions.TruncateTime(DateTime.Now)
                            )
                            .ToList();
                        foreach (var registration in expiredAppUsers)
                        {
                            registration.IsActive = false;
                            registration.DeactivatedById = adminId;
                            registration.DeactivatedRemarks = "AppUser has expired";
                            db.Entry(registration).State = EntityState.Modified;
                        }

                        db.SaveChanges();
                        transaction.Commit();

                        Log.Information($"DeactivateExpiredAppUsersJob executed successfully at {DateTime.Now}. Deactivated {expiredAppUsers.Count} expired registrations.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Log.Error(ex, $"An error occurred while executing DeactivateExpiredAppUsersJob at {DateTime.Now}.");
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}