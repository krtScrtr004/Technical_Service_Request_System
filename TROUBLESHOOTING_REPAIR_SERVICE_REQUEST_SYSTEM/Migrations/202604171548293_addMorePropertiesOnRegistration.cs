namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addMorePropertiesOnRegistration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Registrations", "ExtensionName", c => c.String());
            AddColumn("dbo.Registrations", "Office", c => c.String());
            AddColumn("dbo.Registrations", "Position", c => c.String());
            AddColumn("dbo.RegistrationRequests", "ExtensionName", c => c.String());
            AddColumn("dbo.RegistrationRequests", "Office", c => c.String());
            AddColumn("dbo.RegistrationRequests", "Position", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RegistrationRequests", "Position");
            DropColumn("dbo.RegistrationRequests", "Office");
            DropColumn("dbo.RegistrationRequests", "ExtensionName");
            DropColumn("dbo.Registrations", "Position");
            DropColumn("dbo.Registrations", "Office");
            DropColumn("dbo.Registrations", "ExtensionName");
        }
    }
}
