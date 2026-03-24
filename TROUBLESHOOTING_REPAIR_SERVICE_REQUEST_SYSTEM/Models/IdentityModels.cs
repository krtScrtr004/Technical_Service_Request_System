using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Privilege> Privileges { get; set; }
        public DbSet<UserPrivilege> UserPrivileges { get; set; }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<RegistrationRequest> RegistrationRequests { get; set; }
        public DbSet<TechnicalServiceRequest> TechnicalServiceRequests { get; set; }
        public DbSet<TechnicalServiceRequestHistory> TechnicalServiceRequestHistories { get; set; }
        public DbSet<TechnicalServiceRequestSeverity> TechnicalServiceRequestSeverities { get; set; }
        public DbSet<TechnicalServiceRequestStatus> TechnicalServiceRequestStatus { get; set; }
        public DbSet<TechnicalServiceType> TechnicalServiceTypes { get; set; }
        public DbSet<TechnicalServiceRequestQueue> TechnicalServiceRequestQueues { get; set; }
        public DbSet<ITAvailability> ITAvailabilities { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}