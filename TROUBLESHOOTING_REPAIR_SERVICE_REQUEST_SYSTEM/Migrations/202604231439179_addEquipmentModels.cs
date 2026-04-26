namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addEquipmentModels : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EquipmentCategories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EquipmentCategoryName = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.EquipmentCategoryName, unique: true);
            
            CreateTable(
                "dbo.Equipments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EquipmentModel = c.String(),
                        AssetTag = c.String(maxLength: 100),
                        EquipmentTypeId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EquipmentTypes", t => t.EquipmentTypeId)
                .Index(t => t.AssetTag, unique: true)
                .Index(t => t.EquipmentTypeId);
            
            CreateTable(
                "dbo.EquipmentTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EquipmentTypeName = c.String(),
                        EquipmentCategoryId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EquipmentCategories", t => t.EquipmentCategoryId)
                .Index(t => t.EquipmentCategoryId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Equipments", "EquipmentTypeId", "dbo.EquipmentTypes");
            DropForeignKey("dbo.EquipmentTypes", "EquipmentCategoryId", "dbo.EquipmentCategories");
            DropIndex("dbo.EquipmentTypes", new[] { "EquipmentCategoryId" });
            DropIndex("dbo.Equipments", new[] { "EquipmentTypeId" });
            DropIndex("dbo.Equipments", new[] { "AssetTag" });
            DropIndex("dbo.EquipmentCategories", new[] { "EquipmentCategoryName" });
            DropTable("dbo.EquipmentTypes");
            DropTable("dbo.Equipments");
            DropTable("dbo.EquipmentCategories");
        }
    }
}
