namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addCreatedByInEquipmentModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Equipments", "CreatedByRegistrationId", c => c.Int());
            CreateIndex("dbo.Equipments", "CreatedByRegistrationId");
            AddForeignKey("dbo.Equipments", "CreatedByRegistrationId", "dbo.Registrations", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Equipments", "CreatedByRegistrationId", "dbo.Registrations");
            DropIndex("dbo.Equipments", new[] { "CreatedByRegistrationId" });
            DropColumn("dbo.Equipments", "CreatedByRegistrationId");
        }
    }
}
