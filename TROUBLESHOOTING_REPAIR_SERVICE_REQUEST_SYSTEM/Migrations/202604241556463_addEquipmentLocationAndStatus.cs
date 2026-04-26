namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addEquipmentLocationAndStatus : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EquipmentLocations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BuildingNumber = c.Int(nullable: false),
                        FloorNumber = c.Int(nullable: false),
                        Office = c.String(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EquipmentStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EquipmentStatusName = c.String(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Equipments", "EquipmentLocationId", c => c.Int());
            AddColumn("dbo.Equipments", "EquipmentStatusId", c => c.Int());
            AddColumn("dbo.Equipments", "RepairCount", c => c.Int(nullable: false));
            CreateIndex("dbo.Equipments", "EquipmentLocationId");
            CreateIndex("dbo.Equipments", "EquipmentStatusId");
            AddForeignKey("dbo.Equipments", "EquipmentLocationId", "dbo.EquipmentLocations", "Id");
            AddForeignKey("dbo.Equipments", "EquipmentStatusId", "dbo.EquipmentStatus", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Equipments", "EquipmentStatusId", "dbo.EquipmentStatus");
            DropForeignKey("dbo.Equipments", "EquipmentLocationId", "dbo.EquipmentLocations");
            DropIndex("dbo.Equipments", new[] { "EquipmentStatusId" });
            DropIndex("dbo.Equipments", new[] { "EquipmentLocationId" });
            DropColumn("dbo.Equipments", "RepairCount");
            DropColumn("dbo.Equipments", "EquipmentStatusId");
            DropColumn("dbo.Equipments", "EquipmentLocationId");
            DropTable("dbo.EquipmentStatus");
            DropTable("dbo.EquipmentLocations");
        }
    }
}
