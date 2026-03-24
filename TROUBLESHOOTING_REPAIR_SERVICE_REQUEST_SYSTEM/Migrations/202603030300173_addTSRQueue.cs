namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTSRQueue : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TechnicalServiceRequestQueues",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TechnicalServiceRequestId = c.Int(nullable: false),
                        QueuedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TechnicalServiceRequests", t => t.TechnicalServiceRequestId, cascadeDelete: true)
                .Index(t => t.TechnicalServiceRequestId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TechnicalServiceRequestQueues", "TechnicalServiceRequestId", "dbo.TechnicalServiceRequests");
            DropIndex("dbo.TechnicalServiceRequestQueues", new[] { "TechnicalServiceRequestId" });
            DropTable("dbo.TechnicalServiceRequestQueues");
        }
    }
}
