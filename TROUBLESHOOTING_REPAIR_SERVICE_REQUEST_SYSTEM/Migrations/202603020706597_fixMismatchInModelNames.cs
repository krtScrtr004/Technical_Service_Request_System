namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixMismatchInModelNames : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SystemSupportProcessTypes",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    SystemSupportProcessTypeName = c.String(),
                    IsActive = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            AddColumn("dbo.TechnicalServiceRequests", "SystemSupportProcessTypeId", c => c.Int());
            CreateIndex("dbo.TechnicalServiceRequests", "SystemSupportProcessTypeId");
            AddForeignKey("dbo.TechnicalServiceRequests", "SystemSupportProcessTypeId", "dbo.SystemSupportProcessTypes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TechnicalServiceRequests", "SystemSupportProcessTypeId", "dbo.SystemSupportProcessTypes");
            DropIndex("dbo.TechnicalServiceRequests", new[] { "SystemSupportProcessTypeId" });
            DropColumn("dbo.TechnicalServiceRequests", "SystemSupportProcessTypeId");
            DropTable("dbo.SystemSupportProcessTypes");
        }
    }
}
