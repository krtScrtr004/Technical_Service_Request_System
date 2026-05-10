namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Serilog;
    using System;
    using System.Configuration;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Enumerables;
    using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models.ApplicationDbContext>
    {
        readonly int SUPER_ADMIN_ID;
        readonly string SUPER_ADMIN_FIRST_NAME;
        readonly string SUPER_ADMIN_MIDDLE_NAME;
        readonly string SUPER_ADMIN_LAST_NAME;
        readonly string SUPER_ADMIN_CONTACT_NUMBER;
        readonly string SUPER_ADMIN_EMAIL;
        readonly string SUPER_ADMIN_CODE;
        readonly string SUPER_ADMIN_OFFICE;
        readonly string SUPER_ADMIN_POSITION;

        public Configuration()
        {
            AutomaticMigrationsEnabled = false;

            SUPER_ADMIN_ID = int.TryParse(ConfigurationManager.AppSettings["SUPER_ADMIN_ID"], out var id) ? id : 1;
            SUPER_ADMIN_FIRST_NAME = ConfigurationManager.AppSettings["SUPER_ADMIN_FIRST_NAME"];
            SUPER_ADMIN_MIDDLE_NAME = ConfigurationManager.AppSettings["SUPER_ADMIN_MIDDLE_NAME"];
            SUPER_ADMIN_LAST_NAME = ConfigurationManager.AppSettings["SUPER_ADMIN_LAST_NAME"];
            SUPER_ADMIN_CONTACT_NUMBER = ConfigurationManager.AppSettings["SUPER_ADMIN_CONTACT_NUMBER"];

            SUPER_ADMIN_EMAIL = ConfigurationManager.AppSettings["SUPER_ADMIN_EMAIL"];
            SUPER_ADMIN_CODE = ConfigurationManager.AppSettings["SUPER_ADMIN_CODE"];

            SUPER_ADMIN_OFFICE = ConfigurationManager.AppSettings["SUPER_ADMIN_OFFICE"];
            SUPER_ADMIN_POSITION = ConfigurationManager.AppSettings["SUPER_ADMIN_POSITION"];
        }

        protected override void Seed(ApplicationDbContext context)
        {
            Log.Information("Starting migration seed for super admin user.");

            if (string.IsNullOrEmpty(SUPER_ADMIN_EMAIL) || string.IsNullOrEmpty(SUPER_ADMIN_CODE))
            {
                Log.Warning("Super admin environment variables are not set. Skipping seed.");
                return;
            }

            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    CreateSuperAdminASPApplicationUser(context);
                    CreateSuperAdminAppUserRegistration(context);
                    CreateSuperAdminAppUser(context);

                    context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Log.Error(ex, "An error occurred while seeding the super admin user.");

                    throw; // Re-throw the exception to ensure the migration fails and doesn't leave the database in an inconsistent state
                }
            }
        }

        // Create the ASP.NET Identity user and role for the super admin
        private void CreateSuperAdminASPApplicationUser(ApplicationDbContext context)
        {
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));

            // Create ASP.NET Identity super admin user if it doesn't exist
            if (!context.Users.Any(u => u.Email == SUPER_ADMIN_EMAIL))
            {
                var user = new ApplicationUser
                {
                    UserName = SUPER_ADMIN_EMAIL,
                    Email = SUPER_ADMIN_EMAIL,
                    EmailConfirmed = true
                };
                userManager.Create(user, SUPER_ADMIN_CODE);
            }

            // Create the admin role if it doesn't exist
            var roleName = AppUserRoleEnum.DisplayName(AppUserRoleEnum.ADMIN);
            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            if (!RoleManager.RoleExists(roleName))
            {
                var role = new IdentityRole(roleName);
                RoleManager.Create(role);
            }
        }

        // Create the AppUserRegistration entry for the super admin
        private void CreateSuperAdminAppUserRegistration(ApplicationDbContext context)
        {
            if (!context.AppUserRegistrations.Any(e => e.Id == SUPER_ADMIN_ID || e.Email == SUPER_ADMIN_EMAIL))
            {
                var registration = new AppUserRegistration
                {
                    Id = SUPER_ADMIN_ID,
                    FirstName = SUPER_ADMIN_FIRST_NAME,
                    MiddleName = SUPER_ADMIN_MIDDLE_NAME,
                    LastName = SUPER_ADMIN_LAST_NAME,
                    ContactNumber = SUPER_ADMIN_CONTACT_NUMBER,
                    Office = SUPER_ADMIN_OFFICE,
                    Position = SUPER_ADMIN_POSITION,
                    Email = SUPER_ADMIN_EMAIL,
                    RequestDate = DateTime.Now,
                    IsApproved = true
                };
                context.AppUserRegistrations.Add(registration);
            }
        }

        // Create the AppUser entry for the super admin
        private void CreateSuperAdminAppUser(ApplicationDbContext context)
        {
            if (!context.AppUsers.Any(e => e.Id == SUPER_ADMIN_ID || e.Email == SUPER_ADMIN_EMAIL))
            {
                var appUser = new AppUser
                {
                    Id = SUPER_ADMIN_ID,
                    FirstName = SUPER_ADMIN_FIRST_NAME,
                    MiddleName = SUPER_ADMIN_MIDDLE_NAME,
                    LastName = SUPER_ADMIN_LAST_NAME,
                    ContactNumber = SUPER_ADMIN_CONTACT_NUMBER,
                    Office = SUPER_ADMIN_OFFICE,
                    Position = SUPER_ADMIN_POSITION,
                    Email = SUPER_ADMIN_EMAIL,
                    Code = SUPER_ADMIN_CODE,
                    RoleId = AppUserRoleEnum.ADMIN,
                    RegistrationId = SUPER_ADMIN_ID,
                    RegistrationDate = DateTime.Now,
                    IsActive = true
                };
                context.AppUsers.Add(appUser);

            }
        }
    }

}
