namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addNotification : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RecipientRegistrationId = c.Int(),
                        Title = c.String(),
                        Message = c.String(),
                        IsAll = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Registrations", t => t.RecipientRegistrationId)
                .Index(t => t.RecipientRegistrationId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Notifications", "RecipientRegistrationId", "dbo.Registrations");
            DropIndex("dbo.Notifications", new[] { "RecipientRegistrationId" });
            DropTable("dbo.Notifications");
        }
    }
}
