namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class makeReferenceCodeUnique : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.TechnicalServiceRequests", new[] { "ReferenceCode" });
            AlterColumn("dbo.TechnicalServiceRequests", "ReferenceCode", c => c.String(maxLength: 450));
            CreateIndex("dbo.TechnicalServiceRequests", "ReferenceCode", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.TechnicalServiceRequests", new[] { "ReferenceCode" });
            AlterColumn("dbo.TechnicalServiceRequests", "ReferenceCode", c => c.String());
            CreateIndex("dbo.TechnicalServiceRequests", "ReferenceCode", unique: true);
        }
    }
}
