namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addDescriptionToTSR : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TechnicalServiceRequests", "TechnicalServiceRequestDescription", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TechnicalServiceRequests", "TechnicalServiceRequestDescription");
        }
    }
}
