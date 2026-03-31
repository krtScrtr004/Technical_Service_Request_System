namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addIndexesToTables : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.ITAvailabilities", new[] { "RegistrationId" });
            DropIndex("dbo.UserPrivileges", new[] { "RegistrationId" });
            DropIndex("dbo.UserPrivileges", new[] { "PrivilegeId" });
            DropIndex("dbo.Notifications", new[] { "RecipientRegistrationId" });
            DropIndex("dbo.TechnicalServiceRequestHistories", new[] { "TechnicalServiceRequestId" });
            DropIndex("dbo.TechnicalServiceRequestHistories", new[] { "TechnicalServiceRequestStatusId" });
            DropIndex("dbo.TechnicalServiceRequestHistories", new[] { "ActionTakenByRegistrationId" });
            DropIndex("dbo.TechnicalServiceRequests", new[] { "TechnicalServiceTypeId" });
            DropIndex("dbo.TechnicalServiceRequests", new[] { "TechnicalServiceRequestSeverityId" });
            DropIndex("dbo.TechnicalServiceRequests", new[] { "TechnicalServiceRequestStatusId" });
            DropIndex("dbo.TechnicalServiceRequestQueues", new[] { "TechnicalServiceRequestId" });
            AlterColumn("dbo.Registrations", "Email", c => c.String(maxLength: 255));
            AlterColumn("dbo.RegistrationRequests", "Email", c => c.String(maxLength: 255));
            AlterColumn("dbo.TechnicalServiceRequests", "ClientEmailAddress", c => c.String(maxLength: 255));
            CreateIndex("dbo.ITAvailabilities", "RegistrationId");
            CreateIndex("dbo.Registrations", "Email", unique: true);
            CreateIndex("dbo.UserPrivileges", "RegistrationId");
            CreateIndex("dbo.UserPrivileges", "PrivilegeId");
            CreateIndex("dbo.Notifications", "RecipientRegistrationId");
            CreateIndex("dbo.RegistrationRequests", "Email");
            CreateIndex("dbo.TechnicalServiceRequestHistories", "TechnicalServiceRequestId");
            CreateIndex("dbo.TechnicalServiceRequestHistories", "TechnicalServiceRequestStatusId");
            CreateIndex("dbo.TechnicalServiceRequestHistories", "ActionTakenByRegistrationId");
            CreateIndex("dbo.TechnicalServiceRequests", "ClientEmailAddress");
            CreateIndex("dbo.TechnicalServiceRequests", "TechnicalServiceTypeId");
            CreateIndex("dbo.TechnicalServiceRequests", "TechnicalServiceRequestSeverityId");
            CreateIndex("dbo.TechnicalServiceRequests", "TechnicalServiceRequestStatusId");
            CreateIndex("dbo.TechnicalServiceRequests", "TechnicalServiceRequestScheduledDate");
            CreateIndex("dbo.TechnicalServiceRequestQueues", "TechnicalServiceRequestId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.TechnicalServiceRequestQueues", new[] { "TechnicalServiceRequestId" });
            DropIndex("dbo.TechnicalServiceRequests", new[] { "TechnicalServiceRequestScheduledDate" });
            DropIndex("dbo.TechnicalServiceRequests", new[] { "TechnicalServiceRequestStatusId" });
            DropIndex("dbo.TechnicalServiceRequests", new[] { "TechnicalServiceRequestSeverityId" });
            DropIndex("dbo.TechnicalServiceRequests", new[] { "TechnicalServiceTypeId" });
            DropIndex("dbo.TechnicalServiceRequests", new[] { "ClientEmailAddress" });
            DropIndex("dbo.TechnicalServiceRequestHistories", new[] { "ActionTakenByRegistrationId" });
            DropIndex("dbo.TechnicalServiceRequestHistories", new[] { "TechnicalServiceRequestStatusId" });
            DropIndex("dbo.TechnicalServiceRequestHistories", new[] { "TechnicalServiceRequestId" });
            DropIndex("dbo.RegistrationRequests", new[] { "Email" });
            DropIndex("dbo.Notifications", new[] { "RecipientRegistrationId" });
            DropIndex("dbo.UserPrivileges", new[] { "PrivilegeId" });
            DropIndex("dbo.UserPrivileges", new[] { "RegistrationId" });
            DropIndex("dbo.Registrations", new[] { "Email" });
            DropIndex("dbo.ITAvailabilities", new[] { "RegistrationId" });
            AlterColumn("dbo.TechnicalServiceRequests", "ClientEmailAddress", c => c.String());
            AlterColumn("dbo.RegistrationRequests", "Email", c => c.String());
            AlterColumn("dbo.Registrations", "Email", c => c.String());
            CreateIndex("dbo.TechnicalServiceRequestQueues", "TechnicalServiceRequestId");
            CreateIndex("dbo.TechnicalServiceRequests", "TechnicalServiceRequestStatusId");
            CreateIndex("dbo.TechnicalServiceRequests", "TechnicalServiceRequestSeverityId");
            CreateIndex("dbo.TechnicalServiceRequests", "TechnicalServiceTypeId");
            CreateIndex("dbo.TechnicalServiceRequestHistories", "ActionTakenByRegistrationId");
            CreateIndex("dbo.TechnicalServiceRequestHistories", "TechnicalServiceRequestStatusId");
            CreateIndex("dbo.TechnicalServiceRequestHistories", "TechnicalServiceRequestId");
            CreateIndex("dbo.Notifications", "RecipientRegistrationId");
            CreateIndex("dbo.UserPrivileges", "PrivilegeId");
            CreateIndex("dbo.UserPrivileges", "RegistrationId");
            CreateIndex("dbo.ITAvailabilities", "RegistrationId");
        }
    }
}
