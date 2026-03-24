namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addScheduledDateOnTSR : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TechnicalServiceRequests", "TechnicalServiceRequestScheduledDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TechnicalServiceRequests", "TechnicalServiceRequestScheduledDate");
        }
    }
}
