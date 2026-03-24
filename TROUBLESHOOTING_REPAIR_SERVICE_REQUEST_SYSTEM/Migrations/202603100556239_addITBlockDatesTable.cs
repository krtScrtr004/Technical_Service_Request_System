namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addITBlockDatesTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ITBlockDates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RegistrationId = c.Int(nullable: false),
                        BlockDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Registrations", t => t.RegistrationId, cascadeDelete: true)
                .Index(t => t.RegistrationId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ITBlockDates", "RegistrationId", "dbo.Registrations");
            DropIndex("dbo.ITBlockDates", new[] { "RegistrationId" });
            DropTable("dbo.ITBlockDates");
        }
    }
}
