namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addEquipmentIdToTSRModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TechnicalServiceRequests", "TechnicalServiceRequestEquipmentId", c => c.Int());
            CreateIndex("dbo.TechnicalServiceRequests", "TechnicalServiceRequestEquipmentId");
            AddForeignKey("dbo.TechnicalServiceRequests", "TechnicalServiceRequestEquipmentId", "dbo.Equipments", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TechnicalServiceRequests", "TechnicalServiceRequestEquipmentId", "dbo.Equipments");
            DropIndex("dbo.TechnicalServiceRequests", new[] { "TechnicalServiceRequestEquipmentId" });
            DropColumn("dbo.TechnicalServiceRequests", "TechnicalServiceRequestEquipmentId");
        }
    }
}
