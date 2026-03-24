namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addUpdatedAtInTSRHistory : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TechnicalServiceRequestHistories", "UpdatedAt", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TechnicalServiceRequestHistories", "UpdatedAt");
        }
    }
}
