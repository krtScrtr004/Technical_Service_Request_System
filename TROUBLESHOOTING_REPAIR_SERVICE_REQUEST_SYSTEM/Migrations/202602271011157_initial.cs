namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            //CreateTable(
            //    "dbo.Privileges",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            PrivilegeName = c.String(),
            //            ProjectId = c.Int(),
            //        })
            //    .PrimaryKey(t => t.Id)
            //    .ForeignKey("dbo.Projects", t => t.ProjectId)
            //    .Index(t => t.ProjectId);
            
            //CreateTable(
            //    "dbo.Projects",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            ProjectName = c.String(),
            //        })
            //    .PrimaryKey(t => t.Id);
            
            //CreateTable(
            //    "dbo.RegistrationRequests",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            FirstName = c.String(),
            //            MiddleName = c.String(),
            //            LastName = c.String(),
            //            Email = c.String(),
            //            RequestDate = c.DateTime(),
            //            IsVerified = c.Boolean(nullable: false),
            //            IsApproved = c.Boolean(nullable: false),
            //            IsDenied = c.Boolean(nullable: false),
            //            Code = c.String(),
            //            AccountInformation = c.Boolean(nullable: false),
            //            UserPrivilegeInformation = c.Boolean(nullable: false),
            //            EmployeeInformation = c.Boolean(nullable: false),
            //            UserProfilePicture = c.Boolean(nullable: false),
            //            ContactNumber = c.String(),
            //        })
            //    .PrimaryKey(t => t.Id);
            
            //CreateTable(
            //    "dbo.Registrations",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            FirstName = c.String(),
            //            MiddleName = c.String(),
            //            LastName = c.String(),
            //            UserName = c.String(),
            //            Email = c.String(),
            //            ContactNumber = c.String(),
            //            RegistrationDate = c.DateTime(),
            //            ExpiryDate = c.DateTime(),
            //            AccountType = c.String(),
            //            Code = c.String(),
            //            ProjectId = c.Int(),
            //            IsActive = c.Boolean(nullable: false),
            //            IsUpdated = c.Boolean(nullable: false),
            //            RegistrationRequestId = c.Int(),
            //            DeactivatedByRegistrationId = c.Int(),
            //            DeactivatedRemarks = c.String(),
            //            SessionPrivilegeId = c.Int(),
            //        })
            //    .PrimaryKey(t => t.Id)
            //    .ForeignKey("dbo.Projects", t => t.ProjectId)
            //    .Index(t => t.ProjectId);
            
            //CreateTable(
            //    "dbo.AspNetRoles",
            //    c => new
            //        {
            //            Id = c.String(nullable: false, maxLength: 128),
            //            Name = c.String(nullable: false, maxLength: 256),
            //        })
            //    .PrimaryKey(t => t.Id)
            //    .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            //CreateTable(
            //    "dbo.AspNetUserRoles",
            //    c => new
            //        {
            //            UserId = c.String(nullable: false, maxLength: 128),
            //            RoleId = c.String(nullable: false, maxLength: 128),
            //        })
            //    .PrimaryKey(t => new { t.UserId, t.RoleId })
            //    .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
            //    .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
            //    .Index(t => t.UserId)
            //    .Index(t => t.RoleId);
            
            //CreateTable(
            //    "dbo.TechnicalServiceRequestHistories",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            TechnicalServiceRequestId = c.Int(),
            //            TechnicalServiceRequestStatusId = c.Int(),
            //            DateAction = c.DateTime(),
            //            ActionTaken = c.String(),
            //            ActionTakenByRegistrationId = c.Int(),
            //        })
            //    .PrimaryKey(t => t.Id)
            //    .ForeignKey("dbo.Registrations", t => t.ActionTakenByRegistrationId)
            //    .ForeignKey("dbo.TechnicalServiceRequests", t => t.TechnicalServiceRequestId)
            //    .ForeignKey("dbo.TechnicalServiceRequestStatus", t => t.TechnicalServiceRequestStatusId)
            //    .Index(t => t.TechnicalServiceRequestId)
            //    .Index(t => t.TechnicalServiceRequestStatusId)
            //    .Index(t => t.ActionTakenByRegistrationId);
            
            //CreateTable(
            //    "dbo.TechnicalServiceRequests",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            DateRequest = c.DateTime(),
            //            DateReceived = c.DateTime(),
            //            ReferenceCode = c.String(),
            //            ClientLastName = c.String(),
            //            ClientFirstName = c.String(),
            //            ClientMiddleName = c.String(),
            //            ClientExtensionName = c.String(),
            //            ClientOffice = c.String(),
            //            ClientPosition = c.String(),
            //            ClientContactNumber = c.String(),
            //            ClientEmailAddress = c.String(),
            //            TechnicalServiceTypeId = c.Int(),
            //            TechnicalServiceRequestSeverityId = c.Int(),
            //            Others = c.String(),
            //            TechnicalServiceRequestStatusId = c.Int(),
            //        })
            //    .PrimaryKey(t => t.Id)
            //    .ForeignKey("dbo.TechnicalServiceRequestSeverities", t => t.TechnicalServiceRequestSeverityId)
            //    .ForeignKey("dbo.TechnicalServiceRequestStatus", t => t.TechnicalServiceRequestStatusId)
            //    .ForeignKey("dbo.TechnicalServiceTypes", t => t.TechnicalServiceTypeId)
            //    .Index(t => t.TechnicalServiceTypeId)
            //    .Index(t => t.TechnicalServiceRequestSeverityId)
            //    .Index(t => t.TechnicalServiceRequestStatusId);
            
            //CreateTable(
            //    "dbo.TechnicalServiceRequestSeverities",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            SeverityName = c.String(),
            //            Level = c.String(),
            //            TechnicalServiceRequestSeverityDescription = c.String(),
            //            IsActive = c.Boolean(nullable: false),
            //        })
            //    .PrimaryKey(t => t.Id);
            
            //CreateTable(
            //    "dbo.TechnicalServiceRequestStatus",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            TechnicalServiceRequestStatusName = c.String(),
            //            IsActive = c.Boolean(nullable: false),
            //        })
            //    .PrimaryKey(t => t.Id);
            
            //CreateTable(
            //    "dbo.TechnicalServiceTypes",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            TechnicalServiceTypeName = c.String(),
            //            IsActive = c.Boolean(nullable: false),
            //        })
            //    .PrimaryKey(t => t.Id);
            
            //CreateTable(
            //    "dbo.UserPrivileges",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            RegistrationId = c.Int(),
            //            PrivilegeId = c.Int(),
            //        })
            //    .PrimaryKey(t => t.Id)
            //    .ForeignKey("dbo.Privileges", t => t.PrivilegeId)
            //    .ForeignKey("dbo.Registrations", t => t.RegistrationId)
            //    .Index(t => t.RegistrationId)
            //    .Index(t => t.PrivilegeId);
            
            //CreateTable(
            //    "dbo.AspNetUsers",
            //    c => new
            //        {
            //            Id = c.String(nullable: false, maxLength: 128),
            //            Email = c.String(maxLength: 256),
            //            EmailConfirmed = c.Boolean(nullable: false),
            //            PasswordHash = c.String(),
            //            SecurityStamp = c.String(),
            //            PhoneNumber = c.String(),
            //            PhoneNumberConfirmed = c.Boolean(nullable: false),
            //            TwoFactorEnabled = c.Boolean(nullable: false),
            //            LockoutEndDateUtc = c.DateTime(),
            //            LockoutEnabled = c.Boolean(nullable: false),
            //            AccessFailedCount = c.Int(nullable: false),
            //            UserName = c.String(nullable: false, maxLength: 256),
            //        })
            //    .PrimaryKey(t => t.Id)
            //    .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            //CreateTable(
            //    "dbo.AspNetUserClaims",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            UserId = c.String(nullable: false, maxLength: 128),
            //            ClaimType = c.String(),
            //            ClaimValue = c.String(),
            //        })
            //    .PrimaryKey(t => t.Id)
            //    .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
            //    .Index(t => t.UserId);
            
            //CreateTable(
            //    "dbo.AspNetUserLogins",
            //    c => new
            //        {
            //            LoginProvider = c.String(nullable: false, maxLength: 128),
            //            ProviderKey = c.String(nullable: false, maxLength: 128),
            //            UserId = c.String(nullable: false, maxLength: 128),
            //        })
            //    .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
            //    .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
            //    .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserPrivileges", "RegistrationId", "dbo.Registrations");
            DropForeignKey("dbo.UserPrivileges", "PrivilegeId", "dbo.Privileges");
            DropForeignKey("dbo.TechnicalServiceRequestHistories", "TechnicalServiceRequestStatusId", "dbo.TechnicalServiceRequestStatus");
            DropForeignKey("dbo.TechnicalServiceRequestHistories", "TechnicalServiceRequestId", "dbo.TechnicalServiceRequests");
            DropForeignKey("dbo.TechnicalServiceRequests", "TechnicalServiceTypeId", "dbo.TechnicalServiceTypes");
            DropForeignKey("dbo.TechnicalServiceRequests", "TechnicalServiceRequestStatusId", "dbo.TechnicalServiceRequestStatus");
            DropForeignKey("dbo.TechnicalServiceRequests", "TechnicalServiceRequestSeverityId", "dbo.TechnicalServiceRequestSeverities");
            DropForeignKey("dbo.TechnicalServiceRequestHistories", "ActionTakenByRegistrationId", "dbo.Registrations");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Registrations", "ProjectId", "dbo.Projects");
            DropForeignKey("dbo.Privileges", "ProjectId", "dbo.Projects");
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.UserPrivileges", new[] { "PrivilegeId" });
            DropIndex("dbo.UserPrivileges", new[] { "RegistrationId" });
            DropIndex("dbo.TechnicalServiceRequests", new[] { "TechnicalServiceRequestStatusId" });
            DropIndex("dbo.TechnicalServiceRequests", new[] { "TechnicalServiceRequestSeverityId" });
            DropIndex("dbo.TechnicalServiceRequests", new[] { "TechnicalServiceTypeId" });
            DropIndex("dbo.TechnicalServiceRequestHistories", new[] { "ActionTakenByRegistrationId" });
            DropIndex("dbo.TechnicalServiceRequestHistories", new[] { "TechnicalServiceRequestStatusId" });
            DropIndex("dbo.TechnicalServiceRequestHistories", new[] { "TechnicalServiceRequestId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Registrations", new[] { "ProjectId" });
            DropIndex("dbo.Privileges", new[] { "ProjectId" });
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.UserPrivileges");
            DropTable("dbo.TechnicalServiceTypes");
            DropTable("dbo.TechnicalServiceRequestStatus");
            DropTable("dbo.TechnicalServiceRequestSeverities");
            DropTable("dbo.TechnicalServiceRequests");
            DropTable("dbo.TechnicalServiceRequestHistories");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Registrations");
            DropTable("dbo.RegistrationRequests");
            DropTable("dbo.Projects");
            DropTable("dbo.Privileges");
        }
    }
}
