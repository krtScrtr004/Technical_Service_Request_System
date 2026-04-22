namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class referenceClientInfoFromRegistration : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.TechnicalServiceRequests", new[] { "ClientEmailAddress" });
            AddColumn("dbo.TechnicalServiceRequests", "ClientRegistrationId", c => c.Int(nullable: false));
            CreateIndex("dbo.TechnicalServiceRequests", "ClientRegistrationId");
            AddForeignKey("dbo.TechnicalServiceRequests", "ClientRegistrationId", "dbo.Registrations", "Id", cascadeDelete: true);
            DropColumn("dbo.TechnicalServiceRequests", "ClientLastName");
            DropColumn("dbo.TechnicalServiceRequests", "ClientFirstName");
            DropColumn("dbo.TechnicalServiceRequests", "ClientMiddleName");
            DropColumn("dbo.TechnicalServiceRequests", "ClientExtensionName");
            DropColumn("dbo.TechnicalServiceRequests", "ClientOffice");
            DropColumn("dbo.TechnicalServiceRequests", "ClientPosition");
            DropColumn("dbo.TechnicalServiceRequests", "ClientContactNumber");
            DropColumn("dbo.TechnicalServiceRequests", "ClientEmailAddress");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TechnicalServiceRequests", "ClientEmailAddress", c => c.String(maxLength: 255));
            AddColumn("dbo.TechnicalServiceRequests", "ClientContactNumber", c => c.String());
            AddColumn("dbo.TechnicalServiceRequests", "ClientPosition", c => c.String());
            AddColumn("dbo.TechnicalServiceRequests", "ClientOffice", c => c.String());
            AddColumn("dbo.TechnicalServiceRequests", "ClientExtensionName", c => c.String());
            AddColumn("dbo.TechnicalServiceRequests", "ClientMiddleName", c => c.String());
            AddColumn("dbo.TechnicalServiceRequests", "ClientFirstName", c => c.String());
            AddColumn("dbo.TechnicalServiceRequests", "ClientLastName", c => c.String());
            DropForeignKey("dbo.TechnicalServiceRequests", "ClientRegistrationId", "dbo.Registrations");
            DropIndex("dbo.TechnicalServiceRequests", new[] { "ClientRegistrationId" });
            DropColumn("dbo.TechnicalServiceRequests", "ClientRegistrationId");
            CreateIndex("dbo.TechnicalServiceRequests", "ClientEmailAddress");
        }
    }
}
