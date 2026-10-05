namespace Queue.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Agent_ClasificationGroup",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        idprogramclasification = c.Guid(nullable: false),
                        idemployeesGroup = c.Guid(nullable: false),
                        clasification = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Agent_EmployeesGroups", t => t.idemployeesGroup, cascadeDelete: true)
                .ForeignKey("dbo.Agent_ProgramClasification", t => t.idprogramclasification, cascadeDelete: true)
                .Index(t => t.idprogramclasification)
                .Index(t => t.idemployeesGroup);
            
            CreateTable(
                "dbo.Agent_EmployeesGroups",
                c => new
                    {
                        idemployeesGroup = c.Guid(nullable: false),
                        Nombre = c.String(nullable: false),
                        Agent_Empresa_IdCompany = c.Guid(),
                    })
                .PrimaryKey(t => t.idemployeesGroup)
                .ForeignKey("dbo.Agent_Empresa", t => t.Agent_Empresa_IdCompany)
                .Index(t => t.Agent_Empresa_IdCompany);
            
            CreateTable(
                "dbo.Agent_Empresa",
                c => new
                    {
                        IdCompany = c.Guid(nullable: false),
                        Nombre = c.String(nullable: false),
                        Direccion = c.String(nullable: false),
                        Telefono = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        Rut = c.String(nullable: false),
                        Key = c.String(nullable: false),
                        Id_EmpresaBPM = c.Decimal(nullable: false, precision: 18, scale: 2),
                        status = c.Boolean(nullable: false),
                        string_status = c.String(),
                    })
                .PrimaryKey(t => t.IdCompany);
            
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
            
            CreateTable(
                "dbo.Agent_ProgramClasification",
                c => new
                    {
                        idprogramclasification = c.Guid(nullable: false),
                        name = c.String(nullable: false),
                        title = c.String(),
                        clasification = c.Int(nullable: false),
                        Agent_Empresa_IdCompany = c.Guid(),
                    })
                .PrimaryKey(t => t.idprogramclasification)
                .ForeignKey("dbo.Agent_Empresa", t => t.Agent_Empresa_IdCompany)
                .Index(t => t.Agent_Empresa_IdCompany);
            
            CreateTable(
                "dbo.Agent_CompanyDepartment",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        IdCompany = c.Guid(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Agent_Empresa", t => t.IdCompany, cascadeDelete: true)
                .Index(t => t.IdCompany);
            
            CreateTable(
                "dbo.Agent_Configuration",
                c => new
                    {
                        Id_Configuration = c.Guid(nullable: false),
                        InactivityPeriod = c.Int(nullable: false),
                        UploadFrecuency = c.Int(nullable: false),
                        CaptureFrecuency = c.Int(nullable: false),
                        IdCompany = c.Guid(),
                        LocationFrecuency = c.Int(nullable: false),
                        DateCreation = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id_Configuration)
                .ForeignKey("dbo.Agent_Empresa", t => t.IdCompany)
                .Index(t => t.IdCompany);
            
            CreateTable(
                "dbo.Agent_Employee",
                c => new
                    {
                        idEmployee = c.Guid(nullable: false),
                        IdCompany = c.Guid(nullable: false),
                        Nombre = c.String(nullable: false),
                        Direccion = c.String(nullable: false),
                        Telefono = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        Identificacion = c.String(nullable: false),
                        Usuario = c.String(nullable: false),
                        Cargo = c.Guid(nullable: false),
                        status = c.Boolean(nullable: false),
                        string_status = c.String(),
                        Ip = c.String(),
                        CodigoPais = c.String(),
                        Pais = c.String(),
                        Region = c.String(),
                        Ciudad = c.String(),
                        Latitud = c.Decimal(precision: 18, scale: 8),
                        Longitud = c.Decimal(precision: 18, scale: 8),
                        Id_GroupHorary = c.Guid(),
                        Agent_CompanyDepartment_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.idEmployee)
                .ForeignKey("dbo.Agent_CompanyDepartment", t => t.Agent_CompanyDepartment_Id)
                .ForeignKey("dbo.Agent_Horary", t => t.Id_GroupHorary)
                .Index(t => t.Id_GroupHorary)
                .Index(t => t.Agent_CompanyDepartment_Id);
            
            CreateTable(
                "dbo.Agent_EmployeeUsb",
                c => new
                    {
                        EmployeeUsbId = c.Guid(nullable: false),
                        idEmployee = c.Guid(nullable: false),
                        PortId = c.String(),
                        Description = c.String(),
                        Status = c.Boolean(nullable: false),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.EmployeeUsbId)
                .ForeignKey("dbo.Agent_Employee", t => t.idEmployee, cascadeDelete: true)
                .Index(t => t.idEmployee);
            
            CreateTable(
                "dbo.Agent_Horary",
                c => new
                    {
                        Id_GroupHorary = c.Guid(nullable: false),
                        NameGroup = c.String(nullable: false),
                        IdCompany = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id_GroupHorary);
            
            CreateTable(
                "dbo.WorkAreaEmployee",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdWorkArea = c.Guid(nullable: false),
                        idEmployee = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Agent_Employee", t => t.idEmployee, cascadeDelete: true)
                .Index(t => t.idEmployee);
            
            CreateTable(
                "dbo.Agent_EmployeeGroupsEmployee",
                c => new
                    {
                        idAgent_EmployeeGroupsEmployee = c.Guid(nullable: false),
                        Agent_Employee_idEmployee = c.Guid(),
                        Agent_EmployeesGroups_idemployeesGroup = c.Guid(),
                    })
                .PrimaryKey(t => t.idAgent_EmployeeGroupsEmployee)
                .ForeignKey("dbo.Agent_Employee", t => t.Agent_Employee_idEmployee)
                .ForeignKey("dbo.Agent_EmployeesGroups", t => t.Agent_EmployeesGroups_idemployeesGroup)
                .Index(t => t.Agent_Employee_idEmployee)
                .Index(t => t.Agent_EmployeesGroups_idemployeesGroup);
            
            CreateTable(
                "dbo.Agent_GenericError",
                c => new
                    {
                        codigo_id = c.Int(nullable: false, identity: true),
                        Codigo = c.String(),
                        Message = c.String(),
                    })
                .PrimaryKey(t => t.codigo_id);
            
            CreateTable(
                "dbo.Agent_GroupHoraryDetail",
                c => new
                    {
                        Id_GroupHoraryDetail = c.Guid(nullable: false),
                        Day = c.Int(nullable: false),
                        HourFrom = c.DateTime(nullable: false),
                        HourUntil = c.DateTime(nullable: false),
                        Type = c.Int(nullable: false),
                        Id_GroupHorary = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id_GroupHoraryDetail)
                .ForeignKey("dbo.Agent_Horary", t => t.Id_GroupHorary, cascadeDelete: true)
                .Index(t => t.Id_GroupHorary);
            
            CreateTable(
                "dbo.Agent_Job",
                c => new
                    {
                        idJob = c.Guid(nullable: false),
                        IdCompany = c.Guid(nullable: false),
                        Cargo = c.String(nullable: false),
                        Descripcion = c.String(nullable: false),
                        status = c.Boolean(nullable: false),
                        string_status = c.String(),
                        Lunes = c.Boolean(nullable: false),
                        Masrtes = c.Boolean(nullable: false),
                        Miercoles = c.Boolean(nullable: false),
                        Jueves = c.Boolean(nullable: false),
                        Viernes = c.Boolean(nullable: false),
                        Sabado = c.Boolean(nullable: false),
                        Domingo = c.Boolean(nullable: false),
                        HorarioIniciaLunes = c.DateTime(),
                        HorarioTerminaLunes = c.DateTime(),
                        HorarioIniciaMartes = c.DateTime(),
                        HorarioTerminaMartes = c.DateTime(),
                        HorarioIniciaMiercoles = c.DateTime(),
                        HorarioTerminaMiercoles = c.DateTime(),
                        HorarioIniciaJueves = c.DateTime(),
                        HorarioTerminaJueves = c.DateTime(),
                        HorarioIniciaViernes = c.DateTime(),
                        HorarioTerminaViernes = c.DateTime(),
                        HorarioIniciaSabado = c.DateTime(),
                        HorarioTerminaSabado = c.DateTime(),
                        HorarioIniciaDomingo = c.DateTime(),
                        HorarioTerminaDomingo = c.DateTime(),
                    })
                .PrimaryKey(t => t.idJob);
            
            CreateTable(
                "dbo.Agent_NotificacionUsuario",
                c => new
                    {
                        IdNotificacion = c.Guid(nullable: false),
                        IdCompany = c.Guid(nullable: false),
                        Usuario = c.String(nullable: false),
                        Descripcion = c.String(nullable: false),
                        status = c.Boolean(nullable: false),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.IdNotificacion);
            
            CreateTable(
                "dbo.Agent_UserCompanies",
                c => new
                    {
                        idUser = c.Guid(nullable: false),
                        IdCompany = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.idUser);
            
            CreateTable(
                "dbo.Alertas",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Alerta = c.String(),
                        type = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AlertAsociados",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Email = c.String(),
                        IdCompany = c.Guid(nullable: false),
                        Alertas_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Agent_Empresa", t => t.IdCompany, cascadeDelete: true)
                .ForeignKey("dbo.Alertas", t => t.Alertas_Id)
                .Index(t => t.IdCompany)
                .Index(t => t.Alertas_Id);
            
            CreateTable(
                "dbo.Alerts",
                c => new
                    {
                        IdAlerts = c.Guid(nullable: false),
                        tipo = c.Int(nullable: false),
                        date = c.DateTime(nullable: false),
                        status = c.Boolean(nullable: false),
                        Agent_Employee_idEmployee = c.Guid(),
                    })
                .PrimaryKey(t => t.IdAlerts)
                .ForeignKey("dbo.Agent_Employee", t => t.Agent_Employee_idEmployee)
                .Index(t => t.Agent_Employee_idEmployee);
            
            CreateTable(
                "dbo.License",
                c => new
                    {
                        IdLicense = c.Guid(nullable: false),
                        enddate = c.DateTime(nullable: false),
                        Agent_Empresa_IdCompany = c.Guid(),
                    })
                .PrimaryKey(t => t.IdLicense)
                .ForeignKey("dbo.Agent_Empresa", t => t.Agent_Empresa_IdCompany)
                .Index(t => t.Agent_Empresa_IdCompany);
            
            CreateTable(
                "dbo.LicensePrograms",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        idprogramclasification = c.Guid(nullable: false),
                        isLicensed = c.Boolean(nullable: false),
                        licenseNumber = c.String(),
                        creationDate = c.DateTime(nullable: false),
                        modifyDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Agent_ProgramClasification", t => t.idprogramclasification, cascadeDelete: true)
                .Index(t => t.idprogramclasification);
            
            CreateTable(
                "dbo.Location",
                c => new
                    {
                        LocationID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ClosingTime = c.String(),
                    })
                .PrimaryKey(t => t.LocationID);
            
            CreateTable(
                "dbo.LogApi",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        Thread = c.String(nullable: false, maxLength: 255),
                        Level = c.String(nullable: false, maxLength: 50),
                        Logger = c.String(nullable: false, maxLength: 255),
                        Message = c.String(nullable: false),
                        Exception = c.String(maxLength: 2000),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.LogCodes",
                c => new
                    {
                        LogCode = c.Int(nullable: false, identity: true),
                        LogDescription = c.String(nullable: false, maxLength: 300),
                    })
                .PrimaryKey(t => t.LogCode);
            
            CreateTable(
                "dbo.Printer",
                c => new
                    {
                        PrinterID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        code = c.String(),
                        Status = c.Boolean(nullable: false),
                        location_LocationID = c.Int(),
                    })
                .PrimaryKey(t => t.PrinterID);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                        Discriminator = c.String(nullable: false, maxLength: 128),
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
                "dbo.RoleViewModel",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserLocation",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        IdEmployee = c.Guid(nullable: false),
                        IdCompany = c.Guid(nullable: false),
                        Ip = c.String(),
                        CountryCode = c.String(),
                        CountryName = c.String(),
                        RegionName = c.String(),
                        CityName = c.String(),
                        Latitude = c.Decimal(nullable: false, precision: 18, scale: 8),
                        Longitude = c.Decimal(nullable: false, precision: 18, scale: 8),
                        ZipCode = c.String(),
                        TimeZone = c.String(),
                        MobileBrand = c.String(),
                        Elevation = c.String(),
                        Fecha = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(),
                        LastName = c.String(),
                        IsLoged = c.Boolean(nullable: false),
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
            
            CreateTable(
                "dbo.WorkArea",
                c => new
                    {
                        IdWorkArea = c.Int(nullable: false, identity: true),
                        IdCompany = c.Guid(nullable: false),
                        WorkAreaName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.IdWorkArea);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.LicensePrograms", "idprogramclasification", "dbo.Agent_ProgramClasification");
            DropForeignKey("dbo.License", "Agent_Empresa_IdCompany", "dbo.Agent_Empresa");
            DropForeignKey("dbo.Alerts", "Agent_Employee_idEmployee", "dbo.Agent_Employee");
            DropForeignKey("dbo.AlertAsociados", "Alertas_Id", "dbo.Alertas");
            DropForeignKey("dbo.AlertAsociados", "IdCompany", "dbo.Agent_Empresa");
            DropForeignKey("dbo.Agent_GroupHoraryDetail", "Id_GroupHorary", "dbo.Agent_Horary");
            DropForeignKey("dbo.Agent_EmployeeGroupsEmployee", "Agent_EmployeesGroups_idemployeesGroup", "dbo.Agent_EmployeesGroups");
            DropForeignKey("dbo.Agent_EmployeeGroupsEmployee", "Agent_Employee_idEmployee", "dbo.Agent_Employee");
            DropForeignKey("dbo.WorkAreaEmployee", "idEmployee", "dbo.Agent_Employee");
            DropForeignKey("dbo.Agent_Employee", "Id_GroupHorary", "dbo.Agent_Horary");
            DropForeignKey("dbo.Agent_EmployeeUsb", "idEmployee", "dbo.Agent_Employee");
            DropForeignKey("dbo.Agent_Employee", "Agent_CompanyDepartment_Id", "dbo.Agent_CompanyDepartment");
            DropForeignKey("dbo.Agent_Configuration", "IdCompany", "dbo.Agent_Empresa");
            DropForeignKey("dbo.Agent_CompanyDepartment", "IdCompany", "dbo.Agent_Empresa");
            DropForeignKey("dbo.Agent_ClasificationGroup", "idprogramclasification", "dbo.Agent_ProgramClasification");
            DropForeignKey("dbo.Agent_ProgramClasification", "Agent_Empresa_IdCompany", "dbo.Agent_Empresa");
            DropForeignKey("dbo.Agent_ClasificationGroup", "idemployeesGroup", "dbo.Agent_EmployeesGroups");
            DropForeignKey("dbo.Agent_EmployeesGroups", "Agent_Empresa_IdCompany", "dbo.Agent_Empresa");
            DropForeignKey("dbo.CompanyFunctionality", "IdFunctionality", "dbo.Functionality");
            DropForeignKey("dbo.CompanyFunctionality", "IdCompany", "dbo.Agent_Empresa");
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
            DropIndex("dbo.Agent_GroupHoraryDetail", new[] { "Id_GroupHorary" });
            DropIndex("dbo.Agent_EmployeeGroupsEmployee", new[] { "Agent_EmployeesGroups_idemployeesGroup" });
            DropIndex("dbo.Agent_EmployeeGroupsEmployee", new[] { "Agent_Employee_idEmployee" });
            DropIndex("dbo.WorkAreaEmployee", new[] { "idEmployee" });
            DropIndex("dbo.Agent_EmployeeUsb", new[] { "idEmployee" });
            DropIndex("dbo.Agent_Employee", new[] { "Agent_CompanyDepartment_Id" });
            DropIndex("dbo.Agent_Employee", new[] { "Id_GroupHorary" });
            DropIndex("dbo.Agent_Configuration", new[] { "IdCompany" });
            DropIndex("dbo.Agent_CompanyDepartment", new[] { "IdCompany" });
            DropIndex("dbo.Agent_ProgramClasification", new[] { "Agent_Empresa_IdCompany" });
            DropIndex("dbo.CompanyFunctionality", new[] { "IdFunctionality" });
            DropIndex("dbo.CompanyFunctionality", new[] { "IdCompany" });
            DropIndex("dbo.Agent_EmployeesGroups", new[] { "Agent_Empresa_IdCompany" });
            DropIndex("dbo.Agent_ClasificationGroup", new[] { "idemployeesGroup" });
            DropIndex("dbo.Agent_ClasificationGroup", new[] { "idprogramclasification" });
            DropTable("dbo.WorkArea");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.UserLocation");
            DropTable("dbo.RoleViewModel");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Printer");
            DropTable("dbo.LogCodes");
            DropTable("dbo.LogApi");
            DropTable("dbo.Location");
            DropTable("dbo.LicensePrograms");
            DropTable("dbo.License");
            DropTable("dbo.Alerts");
            DropTable("dbo.AlertAsociados");
            DropTable("dbo.Alertas");
            DropTable("dbo.Agent_UserCompanies");
            DropTable("dbo.Agent_NotificacionUsuario");
            DropTable("dbo.Agent_Job");
            DropTable("dbo.Agent_GroupHoraryDetail");
            DropTable("dbo.Agent_GenericError");
            DropTable("dbo.Agent_EmployeeGroupsEmployee");
            DropTable("dbo.WorkAreaEmployee");
            DropTable("dbo.Agent_Horary");
            DropTable("dbo.Agent_EmployeeUsb");
            DropTable("dbo.Agent_Employee");
            DropTable("dbo.Agent_Configuration");
            DropTable("dbo.Agent_CompanyDepartment");
            DropTable("dbo.Agent_ProgramClasification");
            DropTable("dbo.Functionality");
            DropTable("dbo.CompanyFunctionality");
            DropTable("dbo.Agent_Empresa");
            DropTable("dbo.Agent_EmployeesGroups");
            DropTable("dbo.Agent_ClasificationGroup");
        }
    }
}
