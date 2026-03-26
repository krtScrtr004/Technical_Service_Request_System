namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeUnusedProperties : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Registrations", "UserName");
            DropColumn("dbo.Registrations", "SessionPrivilegeId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Registrations", "SessionPrivilegeId", c => c.Int());
            AddColumn("dbo.Registrations", "UserName", c => c.String());
        }
    }
}
