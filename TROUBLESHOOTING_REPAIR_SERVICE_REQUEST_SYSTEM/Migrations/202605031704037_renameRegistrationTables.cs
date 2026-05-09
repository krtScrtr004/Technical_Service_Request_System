namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class renameRegistrationTables : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Registrations", newName: "AppUsers");
            RenameTable(name: "dbo.RegistrationRoles", newName: "AppUserRoles");
            RenameTable(name: "dbo.RegistrationRequests", newName: "AppUserRegistrations");
            RenameColumn(table: "dbo.ITAvailabilities", name: "RegistrationId", newName: "UserId");
            RenameColumn(table: "dbo.Notifications", name: "RecipientRegistrationId", newName: "RecipientId");
            AddColumn("dbo.AppUsers", "RegistrationId", c => c.Int());
            AddColumn("dbo.AppUsers", "DeactivatedById", c => c.Int());
            DropColumn("dbo.AppUsers", "RegistrationRequestId");
            DropColumn("dbo.AppUsers", "DeactivatedByRegistrationId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AppUsers", "DeactivatedByRegistrationId", c => c.Int());
            AddColumn("dbo.AppUsers", "RegistrationRequestId", c => c.Int());
            DropColumn("dbo.AppUsers", "DeactivatedById");
            DropColumn("dbo.AppUsers", "RegistrationId");
            RenameColumn(table: "dbo.Notifications", name: "RecipientId", newName: "RecipientRegistrationId");
            RenameColumn(table: "dbo.ITAvailabilities", name: "UserId", newName: "RegistrationId");
            RenameTable(name: "dbo.AppUserRegistrations", newName: "RegistrationRequests");
            RenameTable(name: "dbo.AppUserRoles", newName: "RegistrationRoles");
            RenameTable(name: "dbo.AppUsers", newName: "Registrations");
        }
    }
}
