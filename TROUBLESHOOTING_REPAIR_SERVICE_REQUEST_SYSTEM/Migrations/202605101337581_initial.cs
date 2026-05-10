namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AppUserRegistrations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        MiddleName = c.String(),
                        LastName = c.String(),
                        ExtensionName = c.String(),
                        Email = c.String(maxLength: 255),
                        ContactNumber = c.String(),
                        Code = c.String(),
                        Office = c.String(),
                        Position = c.String(),
                        IsApproved = c.Boolean(nullable: false),
                        IsDenied = c.Boolean(nullable: false),
                        RequestDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Email);
            
            CreateTable(
                "dbo.AppUserRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AppUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        MiddleName = c.String(),
                        LastName = c.String(),
                        ExtensionName = c.String(),
                        UserName = c.String(),
                        Email = c.String(maxLength: 255),
                        ContactNumber = c.String(),
                        RegistrationDate = c.DateTime(),
                        ExpiryDate = c.DateTime(),
                        Code = c.String(),
                        RoleId = c.Int(nullable: false),
                        Office = c.String(),
                        Position = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        RegistrationId = c.Int(),
                        DeactivatedById = c.Int(),
                        DeactivatedRemarks = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AppUserRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.Email, unique: true)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.ITAvailabilities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        BlockDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AppUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.EquipmentCategories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
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
                "dbo.Equipments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Model = c.String(),
                        AssetTag = c.String(maxLength: 100),
                        TypeId = c.Int(),
                        LocationId = c.Int(),
                        StatusId = c.Int(),
                        CreatedById = c.Int(),
                        RepairCount = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AppUsers", t => t.CreatedById)
                .ForeignKey("dbo.EquipmentLocations", t => t.LocationId)
                .ForeignKey("dbo.EquipmentStatus", t => t.StatusId)
                .ForeignKey("dbo.EquipmentTypes", t => t.TypeId)
                .Index(t => t.AssetTag, unique: true)
                .Index(t => t.TypeId)
                .Index(t => t.LocationId)
                .Index(t => t.StatusId)
                .Index(t => t.CreatedById);
            
            CreateTable(
                "dbo.EquipmentStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EquipmentTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        CategoryId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EquipmentCategories", t => t.CategoryId)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RecipientId = c.Int(),
                        Title = c.String(),
                        Message = c.String(),
                        ForAdmin = c.Boolean(nullable: false),
                        ForIT = c.Boolean(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AppUsers", t => t.RecipientId)
                .Index(t => t.RecipientId);
            
            CreateTable(
                "dbo.RequestHistories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RequestId = c.Int(),
                        StatusId = c.Int(),
                        DateAction = c.DateTime(),
                        ActionTaken = c.String(),
                        ActionTakenById = c.Int(),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AppUsers", t => t.ActionTakenById)
                .ForeignKey("dbo.Requests", t => t.RequestId)
                .ForeignKey("dbo.RequestStatus", t => t.StatusId)
                .Index(t => t.RequestId)
                .Index(t => t.StatusId)
                .Index(t => t.ActionTakenById);
            
            CreateTable(
                "dbo.Requests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DateRequest = c.DateTime(),
                        DateReceived = c.DateTime(),
                        ReferenceCode = c.String(maxLength: 450),
                        ClientId = c.Int(nullable: false),
                        TypeId = c.Int(),
                        SeverityId = c.Int(),
                        StatusId = c.Int(),
                        Others = c.String(),
                        Description = c.String(),
                        EquipmentId = c.Int(),
                        ScheduledControlProcessDetailId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AppUsers", t => t.ClientId, cascadeDelete: true)
                .ForeignKey("dbo.Equipments", t => t.EquipmentId)
                .ForeignKey("dbo.ScheduledControlProcessDetails", t => t.ScheduledControlProcessDetailId)
                .ForeignKey("dbo.RequestSeverities", t => t.SeverityId)
                .ForeignKey("dbo.RequestStatus", t => t.StatusId)
                .ForeignKey("dbo.RequestTypes", t => t.TypeId)
                .Index(t => t.ReferenceCode, unique: true)
                .Index(t => t.ClientId)
                .Index(t => t.TypeId)
                .Index(t => t.SeverityId)
                .Index(t => t.StatusId)
                .Index(t => t.EquipmentId)
                .Index(t => t.ScheduledControlProcessDetailId);
            
            CreateTable(
                "dbo.ScheduledControlProcessDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ScheduledDate = c.DateTime(),
                        ScheduledStartTime = c.Time(precision: 7),
                        ScheduledEndTime = c.Time(precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.ScheduledDate);
            
            CreateTable(
                "dbo.RequestSeverities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Level = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.RequestStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.RequestTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.RequestQueues",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RequestId = c.Int(nullable: false),
                        QueuedAt = c.DateTime(nullable: false),
                        IsProcessed = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Requests", t => t.RequestId, cascadeDelete: true)
                .Index(t => t.RequestId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.RequestQueues", "RequestId", "dbo.Requests");
            DropForeignKey("dbo.RequestHistories", "StatusId", "dbo.RequestStatus");
            DropForeignKey("dbo.Requests", "TypeId", "dbo.RequestTypes");
            DropForeignKey("dbo.Requests", "StatusId", "dbo.RequestStatus");
            DropForeignKey("dbo.Requests", "SeverityId", "dbo.RequestSeverities");
            DropForeignKey("dbo.Requests", "ScheduledControlProcessDetailId", "dbo.ScheduledControlProcessDetails");
            DropForeignKey("dbo.RequestHistories", "RequestId", "dbo.Requests");
            DropForeignKey("dbo.Requests", "EquipmentId", "dbo.Equipments");
            DropForeignKey("dbo.Requests", "ClientId", "dbo.AppUsers");
            DropForeignKey("dbo.RequestHistories", "ActionTakenById", "dbo.AppUsers");
            DropForeignKey("dbo.Notifications", "RecipientId", "dbo.AppUsers");
            DropForeignKey("dbo.Equipments", "TypeId", "dbo.EquipmentTypes");
            DropForeignKey("dbo.EquipmentTypes", "CategoryId", "dbo.EquipmentCategories");
            DropForeignKey("dbo.Equipments", "StatusId", "dbo.EquipmentStatus");
            DropForeignKey("dbo.Equipments", "LocationId", "dbo.EquipmentLocations");
            DropForeignKey("dbo.Equipments", "CreatedById", "dbo.AppUsers");
            DropForeignKey("dbo.AppUsers", "RoleId", "dbo.AppUserRoles");
            DropForeignKey("dbo.ITAvailabilities", "UserId", "dbo.AppUsers");
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.RequestQueues", new[] { "RequestId" });
            DropIndex("dbo.ScheduledControlProcessDetails", new[] { "ScheduledDate" });
            DropIndex("dbo.Requests", new[] { "ScheduledControlProcessDetailId" });
            DropIndex("dbo.Requests", new[] { "EquipmentId" });
            DropIndex("dbo.Requests", new[] { "StatusId" });
            DropIndex("dbo.Requests", new[] { "SeverityId" });
            DropIndex("dbo.Requests", new[] { "TypeId" });
            DropIndex("dbo.Requests", new[] { "ClientId" });
            DropIndex("dbo.Requests", new[] { "ReferenceCode" });
            DropIndex("dbo.RequestHistories", new[] { "ActionTakenById" });
            DropIndex("dbo.RequestHistories", new[] { "StatusId" });
            DropIndex("dbo.RequestHistories", new[] { "RequestId" });
            DropIndex("dbo.Notifications", new[] { "RecipientId" });
            DropIndex("dbo.EquipmentTypes", new[] { "CategoryId" });
            DropIndex("dbo.Equipments", new[] { "CreatedById" });
            DropIndex("dbo.Equipments", new[] { "StatusId" });
            DropIndex("dbo.Equipments", new[] { "LocationId" });
            DropIndex("dbo.Equipments", new[] { "TypeId" });
            DropIndex("dbo.Equipments", new[] { "AssetTag" });
            DropIndex("dbo.EquipmentCategories", new[] { "Name" });
            DropIndex("dbo.ITAvailabilities", new[] { "UserId" });
            DropIndex("dbo.AppUsers", new[] { "RoleId" });
            DropIndex("dbo.AppUsers", new[] { "Email" });
            DropIndex("dbo.AppUserRegistrations", new[] { "Email" });
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.RequestQueues");
            DropTable("dbo.RequestTypes");
            DropTable("dbo.RequestStatus");
            DropTable("dbo.RequestSeverities");
            DropTable("dbo.ScheduledControlProcessDetails");
            DropTable("dbo.Requests");
            DropTable("dbo.RequestHistories");
            DropTable("dbo.Notifications");
            DropTable("dbo.EquipmentTypes");
            DropTable("dbo.EquipmentStatus");
            DropTable("dbo.Equipments");
            DropTable("dbo.EquipmentLocations");
            DropTable("dbo.EquipmentCategories");
            DropTable("dbo.ITAvailabilities");
            DropTable("dbo.AppUsers");
            DropTable("dbo.AppUserRoles");
            DropTable("dbo.AppUserRegistrations");
        }
    }
}
