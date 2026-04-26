namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class separteScheduledControllProcessDetails : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.TechnicalServiceRequests", new[] { "TechnicalServiceRequestScheduledDate" });
            CreateTable(
                "dbo.ScheduledControlProcessDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TechnicalServiceRequestScheduledDate = c.DateTime(),
                        TechnicalServiceRequestScheduledStartTime = c.Time(precision: 7),
                        TechnicalServiceRequestScheduledEndTime = c.Time(precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.TechnicalServiceRequestScheduledDate);
            
            AddColumn("dbo.TechnicalServiceRequests", "ScheduledControlProcessDetailId", c => c.Int());
            CreateIndex("dbo.TechnicalServiceRequests", "ScheduledControlProcessDetailId");
            AddForeignKey("dbo.TechnicalServiceRequests", "ScheduledControlProcessDetailId", "dbo.ScheduledControlProcessDetails", "Id");
            DropColumn("dbo.TechnicalServiceRequests", "TechnicalServiceRequestScheduledDate");
            DropColumn("dbo.TechnicalServiceRequests", "TechnicalServiceRequestScheduledStartTime");
            DropColumn("dbo.TechnicalServiceRequests", "TechnicalServiceRequestScheduledEndTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TechnicalServiceRequests", "TechnicalServiceRequestScheduledEndTime", c => c.Time(precision: 7));
            AddColumn("dbo.TechnicalServiceRequests", "TechnicalServiceRequestScheduledStartTime", c => c.Time(precision: 7));
            AddColumn("dbo.TechnicalServiceRequests", "TechnicalServiceRequestScheduledDate", c => c.DateTime());
            DropForeignKey("dbo.TechnicalServiceRequests", "ScheduledControlProcessDetailId", "dbo.ScheduledControlProcessDetails");
            DropIndex("dbo.TechnicalServiceRequests", new[] { "ScheduledControlProcessDetailId" });
            DropIndex("dbo.ScheduledControlProcessDetails", new[] { "TechnicalServiceRequestScheduledDate" });
            DropColumn("dbo.TechnicalServiceRequests", "ScheduledControlProcessDetailId");
            DropTable("dbo.ScheduledControlProcessDetails");
            CreateIndex("dbo.TechnicalServiceRequests", "TechnicalServiceRequestScheduledDate");
        }
    }
}
