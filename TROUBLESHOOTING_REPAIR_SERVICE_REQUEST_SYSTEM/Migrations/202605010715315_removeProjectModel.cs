namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeProjectModel : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Registrations", "ProjectId", "dbo.Projects");
            DropForeignKey("dbo.RegistrationRoles", "ProjectId", "dbo.Projects");
            DropIndex("dbo.Registrations", new[] { "ProjectId" });
            DropIndex("dbo.RegistrationRoles", new[] { "ProjectId" });
            DropColumn("dbo.Registrations", "ProjectId");
            DropColumn("dbo.RegistrationRoles", "ProjectId");
            DropTable("dbo.Projects");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Projects",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProjectName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.RegistrationRoles", "ProjectId", c => c.Int());
            AddColumn("dbo.Registrations", "ProjectId", c => c.Int());
            CreateIndex("dbo.RegistrationRoles", "ProjectId");
            CreateIndex("dbo.Registrations", "ProjectId");
            AddForeignKey("dbo.RegistrationRoles", "ProjectId", "dbo.Projects", "Id");
            AddForeignKey("dbo.Registrations", "ProjectId", "dbo.Projects", "Id");
        }
    }
}
