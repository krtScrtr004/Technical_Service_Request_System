namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addStartEndScheduleTimeOnTSR : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TechnicalServiceRequests", "TechnicalServiceRequestScheduledStartTime", c => c.Time(precision: 7));
            AddColumn("dbo.TechnicalServiceRequests", "TechnicalServiceRequestScheduledEndTime", c => c.Time(precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TechnicalServiceRequests", "TechnicalServiceRequestScheduledEndTime");
            DropColumn("dbo.TechnicalServiceRequests", "TechnicalServiceRequestScheduledStartTime");
        }
    }
}
