namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addSystemSupportProcessTypesModel : DbMigration
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
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SystemSupportProcessTypes");
        }
    }
}
