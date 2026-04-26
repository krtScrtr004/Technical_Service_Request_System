namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addAuditProperTiesOnEquipmentModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Equipments", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.Equipments", "CreatedAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.Equipments", "UpdatedAt", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Equipments", "UpdatedAt");
            DropColumn("dbo.Equipments", "CreatedAt");
            DropColumn("dbo.Equipments", "IsActive");
        }
    }
}
