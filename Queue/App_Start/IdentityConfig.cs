using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Queue.Models;
using Queue.DAL;

namespace Queue
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            return Task.FromResult(0);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<QueueContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 4,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = 
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }


    public static class DefaultAdministrator
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(DefaultAdministrator));

        public static void EnsureCreated()
        {
            Log.Info("BootstrapAdmin: EnsureCreated iniciado.");
            var settings = System.Configuration.ConfigurationManager.AppSettings;
            if (!string.Equals(settings["BootstrapAdmin.Enabled"], "true", StringComparison.OrdinalIgnoreCase))
            {
                Log.Warn("BootstrapAdmin: creación omitida porque BootstrapAdmin.Enabled no es true.");
                return;
            }

            var stage = "leer configuración";
            try
            {
                var email = settings["BootstrapAdmin.Email"];
                var password = settings["BootstrapAdmin.Password"];
                var userId = Guid.Parse(settings["BootstrapAdmin.UserId"]);
                var companyId = Guid.Parse(settings["BootstrapAdmin.CompanyId"]);
                var roleId = settings["BootstrapAdmin.RoleId"];
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                    throw new InvalidOperationException("BootstrapAdmin requiere email y contraseña.");

                Log.InfoFormat("BootstrapAdmin: configuración válida; userId={0}; roleId={1}; companyId={2}", userId, roleId, companyId);
                stage = "abrir conexión y transacción";
                Log.Info("BootstrapAdmin: abriendo conexión y transacción de base de datos.");
                using (var db = new QueueContext())
                using (var transaction = db.Database.BeginTransaction())
                using (var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(db)))
                {
                    Log.Info("BootstrapAdmin: transacción abierta.");
                    manager.UserValidator = new UserValidator<ApplicationUser>(manager)
                    {
                        AllowOnlyAlphanumericUserNames = false,
                        RequireUniqueEmail = true
                    };
                    manager.PasswordValidator = new PasswordValidator { RequiredLength = 12 };
                    stage = "buscar rol";
                    Log.InfoFormat("BootstrapAdmin: buscando rol id={0}", roleId);
                    var role = db.Roles.Find(roleId);
                    if (role == null)
                        throw new InvalidOperationException("No existe el rol configurado para BootstrapAdmin.");
                    Log.InfoFormat("BootstrapAdmin: rol encontrado name={0}", role.Name);
                    stage = "buscar empresa";
                    Log.InfoFormat("BootstrapAdmin: buscando empresa id={0}", companyId);
                    if (db.Agent_Empresa.Find(companyId) == null)
                        throw new InvalidOperationException("No existe la empresa configurada para BootstrapAdmin.");
                    Log.Info("BootstrapAdmin: empresa encontrada.");

                    stage = "buscar usuario";
                    Log.Info("BootstrapAdmin: buscando usuario por ID y, si no existe, por email.");
                    var user = manager.FindById(userId.ToString());
                    if (user == null)
                        user = manager.FindByEmail(email);
                    if (user != null && (!string.Equals(user.Id, userId.ToString(), StringComparison.OrdinalIgnoreCase)
                        || !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)))
                        throw new InvalidOperationException("El ID o email configurado pertenece a otro usuario.");

                    var created = user == null;
                    Log.InfoFormat("BootstrapAdmin: búsqueda de usuario finalizada; existe={0}", !created);
                    if (created)
                    {
                        stage = "crear usuario con Identity";
                        Log.Info("BootstrapAdmin: creando usuario y hash de contraseña con Identity.");
                        user = new ApplicationUser
                        {
                            Id = userId.ToString(),
                            UserName = email,
                            Email = email,
                            FirstName = "Administrador",
                            LastName = "Inicial",
                            EmailConfirmed = true,
                            LockoutEnabled = false
                        };
                        RequireSuccess(manager.Create(user, password));
                        Log.InfoFormat("BootstrapAdmin: Identity creó usuario id={0}; pendiente de confirmar transacción.", user.Id);
                    }
                    else
                        Log.Info("BootstrapAdmin: usuario ya existente; se conserva su contraseña.");

                    stage = "asignar rol";
                    if (!manager.IsInRole(user.Id, role.Name))
                    {
                        Log.InfoFormat("BootstrapAdmin: asignando rol={0}", role.Name);
                        RequireSuccess(manager.AddToRole(user.Id, role.Name));
                        Log.Info("BootstrapAdmin: rol asignado.");
                    }
                    else
                        Log.InfoFormat("BootstrapAdmin: usuario ya tiene rol={0}", role.Name);

                    stage = "asociar empresa";
                    var association = db.Agent_UserCompany.Find(userId);
                    if (association != null && association.IdCompany != companyId)
                        throw new InvalidOperationException("El usuario ya está asociado a otra empresa.");
                    if (association == null)
                    {
                        Log.Info("BootstrapAdmin: agregando asociación del usuario con la empresa.");
                        db.Agent_UserCompany.Add(new Agent_UserCompanies { idUser = userId, IdCompany = companyId });
                    }
                    else
                        Log.Info("BootstrapAdmin: asociación con empresa ya existente.");

                    stage = "guardar cambios";
                    Log.Info("BootstrapAdmin: guardando cambios.");
                    db.SaveChanges();
                    stage = "consultar licencia";
                    Log.Info("BootstrapAdmin: comprobando licencia vigente.");
                    var hasLicense = db.License.Any(l => l.Agent_Empresa.IdCompany == companyId && l.enddate >= DateTime.Today);
                    stage = "confirmar transacción";
                    Log.Info("BootstrapAdmin: confirmando transacción.");
                    transaction.Commit();
                    Log.Info("BootstrapAdmin: transacción confirmada; cambios persistidos en la base de datos.");
                    Log.InfoFormat("BootstrapAdmin: usuario {0}; rol={1}; empresa={2}; licenciaVigente={3}",
                        created ? "creado" : "existente", role.Name, companyId, hasLicense);
                    if (!hasLicense)
                        Log.Warn("BootstrapAdmin: el usuario está creado, pero el login requiere una licencia vigente para su empresa.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("BootstrapAdmin: falló la etapa '{0}'; no se confirmó la transacción.", stage), ex);
            }
        }

        private static void RequireSuccess(IdentityResult result)
        {
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors));
        }
    }

    public class ApplicationRoleManager: RoleManager<ApplicationRole>
    {
        public ApplicationRoleManager(IRoleStore<ApplicationRole, string> roleStore) : base(roleStore) { }
        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> Options, IOwinContext context)
        {
            var applicationRoleManager = new ApplicationRoleManager(new RoleStore<ApplicationRole>(context.Get<QueueContext>()));
            return applicationRoleManager;
        }

    }
}
