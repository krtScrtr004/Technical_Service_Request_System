namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addGroupSpecificNotificationProperties : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "ForAdmin", c => c.Boolean(nullable: false));
            AddColumn("dbo.Notifications", "ForIT", c => c.Boolean(nullable: false));
            DropColumn("dbo.Notifications", "IsAll");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Notifications", "IsAll", c => c.Boolean(nullable: false));
            DropColumn("dbo.Notifications", "ForIT");
            DropColumn("dbo.Notifications", "ForAdmin");
        }
    }
}
