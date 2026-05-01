namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class renameDatabaseTables : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Privileges", "ProjectId", "dbo.Projects");
            DropForeignKey("dbo.UserPrivileges", "PrivilegeId", "dbo.Privileges");
            DropForeignKey("dbo.UserPrivileges", "RegistrationId", "dbo.Registrations");
            DropForeignKey("dbo.TechnicalServiceRequestHistories", "ActionTakenByRegistrationId", "dbo.Registrations");
            DropForeignKey("dbo.TechnicalServiceRequests", "ClientRegistrationId", "dbo.Registrations");
            DropForeignKey("dbo.TechnicalServiceRequests", "ScheduledControlProcessDetailId", "dbo.ScheduledControlProcessDetails");
            DropForeignKey("dbo.TechnicalServiceRequests", "TechnicalServiceRequestEquipmentId", "dbo.Equipments");
            DropForeignKey("dbo.TechnicalServiceRequestHistories", "TechnicalServiceRequestId", "dbo.TechnicalServiceRequests");
            DropForeignKey("dbo.TechnicalServiceRequests", "TechnicalServiceRequestSeverityId", "dbo.TechnicalServiceRequestSeverities");
            DropForeignKey("dbo.TechnicalServiceRequests", "TechnicalServiceRequestStatusId", "dbo.TechnicalServiceRequestStatus");
            DropForeignKey("dbo.TechnicalServiceRequests", "TechnicalServiceTypeId", "dbo.TechnicalServiceTypes");
            DropForeignKey("dbo.TechnicalServiceRequestHistories", "TechnicalServiceRequestStatusId", "dbo.TechnicalServiceRequestStatus");
            DropForeignKey("dbo.TechnicalServiceRequestQueues", "TechnicalServiceRequestId", "dbo.TechnicalServiceRequests");
            DropIndex("dbo.EquipmentCategories", new[] { "EquipmentCategoryName" });
            DropIndex("dbo.UserPrivileges", new[] { "RegistrationId" });
            DropIndex("dbo.UserPrivileges", new[] { "PrivilegeId" });
            DropIndex("dbo.Privileges", new[] { "ProjectId" });
            DropIndex("dbo.ScheduledControlProcessDetails", new[] { "TechnicalServiceRequestScheduledDate" });
            DropIndex("dbo.TechnicalServiceRequestHistories", new[] { "TechnicalServiceRequestId" });
            DropIndex("dbo.TechnicalServiceRequestHistories", new[] { "TechnicalServiceRequestStatusId" });
            DropIndex("dbo.TechnicalServiceRequestHistories", new[] { "ActionTakenByRegistrationId" });
            DropIndex("dbo.TechnicalServiceRequests", new[] { "ReferenceCode" });
            DropIndex("dbo.TechnicalServiceRequests", new[] { "ClientRegistrationId" });
            DropIndex("dbo.TechnicalServiceRequests", new[] { "TechnicalServiceTypeId" });
            DropIndex("dbo.TechnicalServiceRequests", new[] { "TechnicalServiceRequestSeverityId" });
            DropIndex("dbo.TechnicalServiceRequests", new[] { "TechnicalServiceRequestStatusId" });
            DropIndex("dbo.TechnicalServiceRequests", new[] { "TechnicalServiceRequestEquipmentId" });
            DropIndex("dbo.TechnicalServiceRequests", new[] { "ScheduledControlProcessDetailId" });
            DropIndex("dbo.TechnicalServiceRequestQueues", new[] { "TechnicalServiceRequestId" });
            RenameColumn(table: "dbo.Equipments", name: "CreatedByRegistrationId", newName: "CreatedById");
            RenameColumn(table: "dbo.Equipments", name: "EquipmentLocationId", newName: "LocationId");
            RenameColumn(table: "dbo.Equipments", name: "EquipmentStatusId", newName: "StatusId");
            RenameColumn(table: "dbo.EquipmentTypes", name: "EquipmentCategoryId", newName: "CategoryId");
            RenameColumn(table: "dbo.Equipments", name: "EquipmentTypeId", newName: "TypeId");
            RenameIndex(table: "dbo.Equipments", name: "IX_EquipmentTypeId", newName: "IX_TypeId");
            RenameIndex(table: "dbo.Equipments", name: "IX_EquipmentLocationId", newName: "IX_LocationId");
            RenameIndex(table: "dbo.Equipments", name: "IX_EquipmentStatusId", newName: "IX_StatusId");
            RenameIndex(table: "dbo.Equipments", name: "IX_CreatedByRegistrationId", newName: "IX_CreatedById");
            RenameIndex(table: "dbo.EquipmentTypes", name: "IX_EquipmentCategoryId", newName: "IX_CategoryId");
            CreateTable(
                "dbo.RegistrationRoles",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    ProjectId = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Projects", t => t.ProjectId)
                .Index(t => t.ProjectId);

            Sql(@"
                SET IDENTITY_INSERT dbo.RegistrationRoles ON;
                INSERT INTO dbo.RegistrationRoles (Id, Name, ProjectId) VALUES (1, 'Administrator', NULL);
                INSERT INTO dbo.RegistrationRoles (Id, Name, ProjectId) VALUES (2, 'IT', NULL);
                INSERT INTO dbo.RegistrationRoles (Id, Name, ProjectId) VALUES (3, 'Standard User', NULL);
                SET IDENTITY_INSERT dbo.RegistrationRoles OFF;");

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
                .ForeignKey("dbo.Registrations", t => t.ActionTakenById)
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
                .ForeignKey("dbo.Registrations", t => t.ClientId, cascadeDelete: true)
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

            Sql(@"UPDATE dbo.EquipmentCategories
SET EquipmentCategoryName = 'UNKNOWN'
WHERE EquipmentCategoryName IS NULL OR LTRIM(RTRIM(EquipmentCategoryName)) = ''");

            RenameColumn("dbo.EquipmentCategories", "EquipmentCategoryName", "Name");
            AlterColumn("dbo.EquipmentCategories", "Name", c => c.String(nullable: false, maxLength: 150));
            RenameColumn("dbo.Equipments", "EquipmentModel", "Model");
            AddColumn("dbo.Registrations", "RoleId", c => c.Int());
            RenameColumn("dbo.EquipmentStatus", "EquipmentStatusName", "Name");
            RenameColumn("dbo.EquipmentTypes", "EquipmentTypeName", "Name");
            AddColumn("dbo.ScheduledControlProcessDetails", "ScheduledDate", c => c.DateTime());
            AddColumn("dbo.ScheduledControlProcessDetails", "ScheduledStartTime", c => c.Time(precision: 7));
            AddColumn("dbo.ScheduledControlProcessDetails", "ScheduledEndTime", c => c.Time(precision: 7));
            CreateIndex("dbo.EquipmentCategories", "Name", unique: true);
            CreateIndex("dbo.ScheduledControlProcessDetails", "ScheduledDate");

            Sql(@"
                UPDATE dbo.Registrations
                SET RoleId = CASE
                    WHEN AccountType = 'Administrator' THEN 1
                    WHEN AccountType = 'IT' THEN 2
                    WHEN AccountType = 'Standard User' THEN 3
                    ELSE 3
                END");

            AlterColumn("dbo.Registrations", "RoleId", c => c.Int(nullable: false));

            CreateIndex("dbo.Registrations", "RoleId");
            AddForeignKey("dbo.Registrations", "RoleId", "dbo.RegistrationRoles", "Id", cascadeDelete: true);
            DropColumn("dbo.Equipments", "IsActive");
            DropColumn("dbo.Registrations", "AccountType");
            DropColumn("dbo.Registrations", "IsUpdated");
            DropColumn("dbo.Registrations", "SessionPrivilegeId");
            DropColumn("dbo.RegistrationRequests", "UserProfilePicture");
            DropColumn("dbo.RegistrationRequests", "IsVerified");
            DropColumn("dbo.RegistrationRequests", "AccountInformation");
            DropColumn("dbo.RegistrationRequests", "UserPrivilegeInformation");
            DropColumn("dbo.RegistrationRequests", "EmployeeInformation");
            DropColumn("dbo.ScheduledControlProcessDetails", "TechnicalServiceRequestScheduledDate");
            DropColumn("dbo.ScheduledControlProcessDetails", "TechnicalServiceRequestScheduledStartTime");
            DropColumn("dbo.ScheduledControlProcessDetails", "TechnicalServiceRequestScheduledEndTime");
            DropTable("dbo.UserPrivileges");
            DropTable("dbo.Privileges");
            DropTable("dbo.TechnicalServiceRequestHistories");
            DropTable("dbo.TechnicalServiceRequests");
            DropTable("dbo.TechnicalServiceRequestSeverities");
            DropTable("dbo.TechnicalServiceRequestStatus");
            DropTable("dbo.TechnicalServiceTypes");
            DropTable("dbo.TechnicalServiceRequestQueues");
        }

        public override void Down()
        {
            CreateTable(
                "dbo.TechnicalServiceRequestQueues",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    TechnicalServiceRequestId = c.Int(nullable: false),
                    QueuedAt = c.DateTime(nullable: false),
                    IsProcessed = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.TechnicalServiceTypes",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    TechnicalServiceTypeName = c.String(),
                    IsActive = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.TechnicalServiceRequestStatus",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    TechnicalServiceRequestStatusName = c.String(),
                    IsActive = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.TechnicalServiceRequestSeverities",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    SeverityName = c.String(),
                    Level = c.String(),
                    TechnicalServiceRequestSeverityDescription = c.String(),
                    IsActive = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.TechnicalServiceRequests",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    DateRequest = c.DateTime(),
                    DateReceived = c.DateTime(),
                    ReferenceCode = c.String(maxLength: 450),
                    ClientRegistrationId = c.Int(nullable: false),
                    TechnicalServiceTypeId = c.Int(),
                    TechnicalServiceRequestSeverityId = c.Int(),
                    TechnicalServiceRequestStatusId = c.Int(),
                    Others = c.String(),
                    TechnicalServiceRequestDescription = c.String(),
                    TechnicalServiceRequestEquipmentId = c.Int(),
                    ScheduledControlProcessDetailId = c.Int(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.TechnicalServiceRequestHistories",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    TechnicalServiceRequestId = c.Int(),
                    TechnicalServiceRequestStatusId = c.Int(),
                    DateAction = c.DateTime(),
                    ActionTaken = c.String(),
                    ActionTakenByRegistrationId = c.Int(),
                    UpdatedAt = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Privileges",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    PrivilegeName = c.String(),
                    ProjectId = c.Int(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.UserPrivileges",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    RegistrationId = c.Int(),
                    PrivilegeId = c.Int(),
                })
                .PrimaryKey(t => t.Id);

            AddColumn("dbo.ScheduledControlProcessDetails", "TechnicalServiceRequestScheduledEndTime", c => c.Time(precision: 7));
            AddColumn("dbo.ScheduledControlProcessDetails", "TechnicalServiceRequestScheduledStartTime", c => c.Time(precision: 7));
            AddColumn("dbo.ScheduledControlProcessDetails", "TechnicalServiceRequestScheduledDate", c => c.DateTime());
            AddColumn("dbo.RegistrationRequests", "EmployeeInformation", c => c.Boolean(nullable: false));
            AddColumn("dbo.RegistrationRequests", "UserPrivilegeInformation", c => c.Boolean(nullable: false));
            AddColumn("dbo.RegistrationRequests", "AccountInformation", c => c.Boolean(nullable: false));
            AddColumn("dbo.RegistrationRequests", "IsVerified", c => c.Boolean(nullable: false));
            AddColumn("dbo.RegistrationRequests", "UserProfilePicture", c => c.Boolean(nullable: false));
            AddColumn("dbo.EquipmentTypes", "EquipmentTypeName", c => c.String());
            AddColumn("dbo.EquipmentStatus", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.EquipmentStatus", "EquipmentStatusName", c => c.String());
            AddColumn("dbo.Registrations", "SessionPrivilegeId", c => c.Int());
            AddColumn("dbo.Registrations", "IsUpdated", c => c.Boolean(nullable: false));
            AddColumn("dbo.Registrations", "AccountType", c => c.String());
            AddColumn("dbo.Equipments", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.Equipments", "EquipmentModel", c => c.String());
            AddColumn("dbo.EquipmentCategories", "EquipmentCategoryName", c => c.String(nullable: false, maxLength: 150));
            DropForeignKey("dbo.RequestQueues", "RequestId", "dbo.Requests");
            DropForeignKey("dbo.RequestHistories", "StatusId", "dbo.RequestStatus");
            DropForeignKey("dbo.Requests", "TypeId", "dbo.RequestTypes");
            DropForeignKey("dbo.Requests", "StatusId", "dbo.RequestStatus");
            DropForeignKey("dbo.Requests", "SeverityId", "dbo.RequestSeverities");
            DropForeignKey("dbo.Requests", "ScheduledControlProcessDetailId", "dbo.ScheduledControlProcessDetails");
            DropForeignKey("dbo.RequestHistories", "RequestId", "dbo.Requests");
            DropForeignKey("dbo.Requests", "EquipmentId", "dbo.Equipments");
            DropForeignKey("dbo.Requests", "ClientId", "dbo.Registrations");
            DropForeignKey("dbo.RequestHistories", "ActionTakenById", "dbo.Registrations");
            DropForeignKey("dbo.Registrations", "RoleId", "dbo.RegistrationRoles");
            DropForeignKey("dbo.RegistrationRoles", "ProjectId", "dbo.Projects");
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
            DropIndex("dbo.RegistrationRoles", new[] { "ProjectId" });
            DropIndex("dbo.Registrations", new[] { "RoleId" });
            DropIndex("dbo.EquipmentCategories", new[] { "Name" });
            DropColumn("dbo.ScheduledControlProcessDetails", "ScheduledEndTime");
            DropColumn("dbo.ScheduledControlProcessDetails", "ScheduledStartTime");
            DropColumn("dbo.ScheduledControlProcessDetails", "ScheduledDate");
            DropColumn("dbo.EquipmentTypes", "Name");
            DropColumn("dbo.EquipmentStatus", "Name");
            DropColumn("dbo.Registrations", "RoleId");
            DropColumn("dbo.Equipments", "Model");
            DropColumn("dbo.EquipmentCategories", "Name");
            DropTable("dbo.RequestQueues");
            DropTable("dbo.RequestTypes");
            DropTable("dbo.RequestStatus");
            DropTable("dbo.RequestSeverities");
            DropTable("dbo.Requests");
            DropTable("dbo.RequestHistories");
            DropTable("dbo.RegistrationRoles");
            RenameIndex(table: "dbo.EquipmentTypes", name: "IX_CategoryId", newName: "IX_EquipmentCategoryId");
            RenameIndex(table: "dbo.Equipments", name: "IX_CreatedById", newName: "IX_CreatedByRegistrationId");
            RenameIndex(table: "dbo.Equipments", name: "IX_StatusId", newName: "IX_EquipmentStatusId");
            RenameIndex(table: "dbo.Equipments", name: "IX_LocationId", newName: "IX_EquipmentLocationId");
            RenameIndex(table: "dbo.Equipments", name: "IX_TypeId", newName: "IX_EquipmentTypeId");
            RenameColumn(table: "dbo.Equipments", name: "TypeId", newName: "EquipmentTypeId");
            RenameColumn(table: "dbo.EquipmentTypes", name: "CategoryId", newName: "EquipmentCategoryId");
            RenameColumn(table: "dbo.Equipments", name: "StatusId", newName: "EquipmentStatusId");
            RenameColumn(table: "dbo.Equipments", name: "LocationId", newName: "EquipmentLocationId");
            RenameColumn(table: "dbo.Equipments", name: "CreatedById", newName: "CreatedByRegistrationId");
            CreateIndex("dbo.TechnicalServiceRequestQueues", "TechnicalServiceRequestId");
            CreateIndex("dbo.TechnicalServiceRequests", "ScheduledControlProcessDetailId");
            CreateIndex("dbo.TechnicalServiceRequests", "TechnicalServiceRequestEquipmentId");
            CreateIndex("dbo.TechnicalServiceRequests", "TechnicalServiceRequestStatusId");
            CreateIndex("dbo.TechnicalServiceRequests", "TechnicalServiceRequestSeverityId");
            CreateIndex("dbo.TechnicalServiceRequests", "TechnicalServiceTypeId");
            CreateIndex("dbo.TechnicalServiceRequests", "ClientRegistrationId");
            CreateIndex("dbo.TechnicalServiceRequests", "ReferenceCode", unique: true);
            CreateIndex("dbo.TechnicalServiceRequestHistories", "ActionTakenByRegistrationId");
            CreateIndex("dbo.TechnicalServiceRequestHistories", "TechnicalServiceRequestStatusId");
            CreateIndex("dbo.TechnicalServiceRequestHistories", "TechnicalServiceRequestId");
            CreateIndex("dbo.ScheduledControlProcessDetails", "TechnicalServiceRequestScheduledDate");
            CreateIndex("dbo.Privileges", "ProjectId");
            CreateIndex("dbo.UserPrivileges", "PrivilegeId");
            CreateIndex("dbo.UserPrivileges", "RegistrationId");
            CreateIndex("dbo.EquipmentCategories", "EquipmentCategoryName", unique: true);
            AddForeignKey("dbo.TechnicalServiceRequestQueues", "TechnicalServiceRequestId", "dbo.TechnicalServiceRequests", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TechnicalServiceRequestHistories", "TechnicalServiceRequestStatusId", "dbo.TechnicalServiceRequestStatus", "Id");
            AddForeignKey("dbo.TechnicalServiceRequests", "TechnicalServiceTypeId", "dbo.TechnicalServiceTypes", "Id");
            AddForeignKey("dbo.TechnicalServiceRequests", "TechnicalServiceRequestStatusId", "dbo.TechnicalServiceRequestStatus", "Id");
            AddForeignKey("dbo.TechnicalServiceRequests", "TechnicalServiceRequestSeverityId", "dbo.TechnicalServiceRequestSeverities", "Id");
            AddForeignKey("dbo.TechnicalServiceRequestHistories", "TechnicalServiceRequestId", "dbo.TechnicalServiceRequests", "Id");
            AddForeignKey("dbo.TechnicalServiceRequests", "TechnicalServiceRequestEquipmentId", "dbo.Equipments", "Id");
            AddForeignKey("dbo.TechnicalServiceRequests", "ScheduledControlProcessDetailId", "dbo.ScheduledControlProcessDetails", "Id");
            AddForeignKey("dbo.TechnicalServiceRequests", "ClientRegistrationId", "dbo.Registrations", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TechnicalServiceRequestHistories", "ActionTakenByRegistrationId", "dbo.Registrations", "Id");
            AddForeignKey("dbo.UserPrivileges", "RegistrationId", "dbo.Registrations", "Id");
            AddForeignKey("dbo.UserPrivileges", "PrivilegeId", "dbo.Privileges", "Id");
            AddForeignKey("dbo.Privileges", "ProjectId", "dbo.Projects", "Id");
        }
    }
}
