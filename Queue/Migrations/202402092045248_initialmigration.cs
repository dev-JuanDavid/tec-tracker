namespace Queue.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initialmigration : DbMigration
    {
        public override void Up()
        {
           
            
           
            
            CreateTable(
                "dbo.CompanyFunctionality",
                c => new
                    {
                        IdCompany = c.Guid(nullable: false),
                        IdFunctionality = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.IdCompany, t.IdFunctionality })
                .ForeignKey("dbo.Agent_Empresa", t => t.IdCompany, cascadeDelete: true)
                .ForeignKey("dbo.Functionality", t => t.IdFunctionality, cascadeDelete: true)
                .Index(t => t.IdCompany)
                .Index(t => t.IdFunctionality);
            
            CreateTable(
                "dbo.Functionality",
                c => new
                    {
                        IdFunctionality = c.Guid(nullable: false, identity: true),
                        Name = c.String(),
                        Active = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.IdFunctionality);
            
           
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.WorkAreaEmployee", "idEmployee", "dbo.Agent_Employee");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.LicensePrograms", "idprogramclasification", "dbo.Agent_ProgramClasification");
            DropForeignKey("dbo.License", "Agent_Empresa_IdCompany", "dbo.Agent_Empresa");
            DropForeignKey("dbo.Alerts", "Agent_Employee_idEmployee", "dbo.Agent_Employee");
            DropForeignKey("dbo.AlertAsociados", "Alertas_Id", "dbo.Alertas");
            DropForeignKey("dbo.AlertAsociados", "IdCompany", "dbo.Agent_Empresa");
            DropForeignKey("dbo.Agent_ProgramClasification", "Agent_Empresa_IdCompany", "dbo.Agent_Empresa");
            DropForeignKey("dbo.Agent_GroupHoraryDetail", "Id_GroupHorary", "dbo.Agent_Horary");
            DropForeignKey("dbo.Agent_EmployeeGroupsEmployee", "Agent_EmployeesGroups_idemployeesGroup", "dbo.Agent_EmployeesGroups");
            DropForeignKey("dbo.Agent_EmployeesGroups", "Agent_Empresa_IdCompany", "dbo.Agent_Empresa");
            DropForeignKey("dbo.Agent_EmployeeGroupsEmployee", "Agent_Employee_idEmployee", "dbo.Agent_Employee");
            DropForeignKey("dbo.Agent_Employee", "Id_GroupHorary", "dbo.Agent_Horary");
            DropForeignKey("dbo.Agent_EmployeeUsb", "idEmployee", "dbo.Agent_Employee");
            DropForeignKey("dbo.Agent_Employee", "Agent_CompanyDepartment_Id", "dbo.Agent_CompanyDepartment");
            DropForeignKey("dbo.Agent_Configuration", "IdCompany", "dbo.Agent_Empresa");
            DropForeignKey("dbo.Agent_CompanyDepartment", "IdCompany", "dbo.Agent_Empresa");
            DropForeignKey("dbo.CompanyFunctionality", "IdFunctionality", "dbo.Functionality");
            DropForeignKey("dbo.CompanyFunctionality", "IdCompany", "dbo.Agent_Empresa");
            DropIndex("dbo.WorkAreaEmployee", new[] { "idEmployee" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.LicensePrograms", new[] { "idprogramclasification" });
            DropIndex("dbo.License", new[] { "Agent_Empresa_IdCompany" });
            DropIndex("dbo.Alerts", new[] { "Agent_Employee_idEmployee" });
            DropIndex("dbo.AlertAsociados", new[] { "Alertas_Id" });
            DropIndex("dbo.AlertAsociados", new[] { "IdCompany" });
            DropIndex("dbo.Agent_ProgramClasification", new[] { "Agent_Empresa_IdCompany" });
            DropIndex("dbo.Agent_GroupHoraryDetail", new[] { "Id_GroupHorary" });
            DropIndex("dbo.Agent_EmployeesGroups", new[] { "Agent_Empresa_IdCompany" });
            DropIndex("dbo.Agent_EmployeeGroupsEmployee", new[] { "Agent_EmployeesGroups_idemployeesGroup" });
            DropIndex("dbo.Agent_EmployeeGroupsEmployee", new[] { "Agent_Employee_idEmployee" });
            DropIndex("dbo.Agent_EmployeeUsb", new[] { "idEmployee" });
            DropIndex("dbo.Agent_Employee", new[] { "Agent_CompanyDepartment_Id" });
            DropIndex("dbo.Agent_Employee", new[] { "Id_GroupHorary" });
            DropIndex("dbo.Agent_Configuration", new[] { "IdCompany" });
            DropIndex("dbo.CompanyFunctionality", new[] { "IdFunctionality" });
            DropIndex("dbo.CompanyFunctionality", new[] { "IdCompany" });
            DropIndex("dbo.Agent_CompanyDepartment", new[] { "IdCompany" });
            DropTable("dbo.WorkAreaEmployee");
            DropTable("dbo.WorkArea");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.UserLocation");
            DropTable("dbo.RoleViewModel");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.LicensePrograms");
            DropTable("dbo.License");
            DropTable("dbo.Alerts");
            DropTable("dbo.AlertAsociados");
            DropTable("dbo.Alertas");
            DropTable("dbo.Agent_UserCompanies");
            DropTable("dbo.Agent_ProgramClasification");
            DropTable("dbo.Agent_Job");
            DropTable("dbo.Agent_GroupHoraryDetail");
            DropTable("dbo.Agent_GenericError");
            DropTable("dbo.Agent_EmployeesGroups");
            DropTable("dbo.Agent_EmployeeGroupsEmployee");
            DropTable("dbo.Agent_Horary");
            DropTable("dbo.Agent_EmployeeUsb");
            DropTable("dbo.Agent_Employee");
            DropTable("dbo.Agent_Configuration");
            DropTable("dbo.Functionality");
            DropTable("dbo.CompanyFunctionality");
            DropTable("dbo.Agent_Empresa");
            DropTable("dbo.Agent_CompanyDepartment");
        }
    }
}
