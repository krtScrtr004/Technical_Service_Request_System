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

        public DbSet<AppUserRole> AppUserRole { get; set; }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<AppUserRegistration> AppUserRegistrations { get; set; }

        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<EquipmentType> EquipmentTypes { get; set; }
        public DbSet<EquipmentCategory> EquipmentCategories { get; set; }
        public DbSet<EquipmentLocation> EquipmentLocations { get; set; }
        public DbSet<EquipmentStatus> EquipmentStatuses { get; set; }

        public DbSet<Request> Requests { get; set; }
        public DbSet<RequestHistory> RequestHistories { get; set; }
        public DbSet<RequestSeverity> RequestSeverities { get; set; }
        public DbSet<RequestStatus> RequestStatuses { get; set; }
        public DbSet<RequestType> Types { get; set; }
        public DbSet<ScheduledControlProcessDetail> ScheduledControlProcessDetails { get; set; }
        public DbSet<RequestQueue> RequestQueues { get; set; }

        public DbSet<ITAvailability> ITAvailabilities { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}