namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class renameITBlockDatesTable : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ITBlockDates", newName: "ITAvailabilities");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.ITAvailabilities", newName: "ITBlockDates");
        }
    }
}
