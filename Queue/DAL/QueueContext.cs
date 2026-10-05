using Queue.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;


namespace Queue.DAL
{
    using global::Queue.Models;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Linq;


    public class QueueContext : IdentityDbContext<ApplicationUser>
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<Agent_Employee>()
                .Property(p => p.Latitud)
                .HasPrecision(18, 8);
            modelBuilder.Entity<Agent_Employee>()
                .Property(p => p.Longitud)
                .HasPrecision(18, 8);

            modelBuilder.Entity<UserLocation>()
                .Property(p => p.Longitude)
                .HasPrecision(18, 8);
            modelBuilder.Entity<UserLocation>()
                .Property(p => p.Latitude)
                .HasPrecision(18, 8);

            modelBuilder.Entity<CompanyFunctionality>()
                .HasKey(x => new { x.IdCompany, x.IdFunctionality });


            modelBuilder.Entity<CompanyFunctionality>()
                .HasRequired(x => x.AgentEmpresa)
                .WithMany(x => x.CompanyFunctionalities)
                .HasForeignKey(x => x.IdCompany);


            modelBuilder.Entity<CompanyFunctionality>()
                .HasRequired(x => x.Functionality)
                .WithMany(x => x.CompanyFunctionalities)
                .HasForeignKey(x => x.IdFunctionality);

            // Conexión Clasificación - grupos
            modelBuilder.Entity<Agent_ClasificationGroup>()
                .HasRequired(x => x.Agent_ProgramClasifications)
                .WithMany(x => x.ClasificationGroups)
                .HasForeignKey(x => x.idprogramclasification);

            modelBuilder.Entity<Agent_ClasificationGroup>()
                .HasRequired(x => x.Agent_EmployeesGroups)
                .WithMany(x => x.ClasificationGroups)
                .HasForeignKey(x => x.idemployeesGroup);

        }


        public QueueContext() : base("name=QueueContext")
        {
            Database.SetInitializer<QueueContext>(null);
        }


        public static QueueContext Create()
        {
            return new QueueContext();
        }

        public DbSet<RoleViewModel> RoleViewModels { get; set; }
        public DbSet<Agent_Employee> Agent_Employee { get; set; }
        public DbSet<UserLocation> UserLocation { get; set; }
        public DbSet<Agent_Job> Agent_Job { get; set; }
        public DbSet<Agent_Empresa> Agent_Empresa { get; set; }
        public DbSet<Agent_GenericError> Agent_GenericError { get; set; }
        public DbSet<Agent_Configuration> Agent_Configuration { get; set; }
        public DbSet<Agent_UserCompanies> Agent_UserCompany { get; set; }
        public DbSet<Agent_ProgramClasification> Agent_ProgramClasification { get; set; }

        public DbSet<Agent_GroupHorary> Agent_GroupHorary { get; set; }
        public DbSet<Agent_GroupHoraryDetail> Agent_GroupHoraryDetail { get; set; }
        public DbSet<Agent_CompanyDepartment> Agent_CompanyDepartment { get; set; }

        public DbSet<Agent_EmployeesGroups> Agent_EmployeesGroups { get; set; }

        public DbSet<Agent_EmployeeGroupsEmployee> Agent_EmployeeGroupsEmployee { get; set; }
        public DbSet<Alerts> Alerts { get; set; }
        public DbSet<License> License { get; set; }
        public DbSet<AlertAsociados> AlertAsociados { get; set; }

        public DbSet<Alertas> Alertas { get; set; }

        public DbSet<Agent_NotificacionUsuario> Agent_NotificacionUsuarios { get; set; }

        //SCI Software
        public DbSet<Agent_EmployeeUsb> Agent_EmployeeUsb { get; set; }
        public DbSet<WorkArea> WorkArea { get; set; }
        public DbSet<WorkAreaEmployee> WorkAreaEmployee { get; set; }
        public DbSet<LicensePrograms> LicensePrograms { get; set; }

        //TecSer Software
        public DbSet<Functionality> Functionality { get; set; }
        public DbSet<CompanyFunctionality> CompanyFunctionality { get; set;}

        // Se agregan los siguientes Modelos para que el EntityFramework los cree en las Migraciones
        public DbSet<Location> Location { get; set; }

        public DbSet<LogApi> LogApi { get; set; }

        public DbSet<LogCodes> LogCodes { get; set; }

        public DbSet<Printer> Printer { get; set; }

        // Organización de Programas con Clasificación y Grupos
        public DbSet<Agent_ClasificationGroup> Agent_ClasificationGroups { get; set; }


    }
}