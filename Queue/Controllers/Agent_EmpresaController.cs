using Microsoft.AspNet.Identity.Owin;
using log4net;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Queue.DAL;
using Queue.Models;
using System.Collections.Generic;
using static Queue.Utils.Enums;
using Queue.Utils;
using System.Data.Entity.Core.Mapping;
using System.Configuration;

namespace Queue.Controllers
{
    [Authorize]
    public class Agent_EmpresaController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Agent_EmpresaController));
        private QueueContext db = new QueueContext();
        private ApplicationUserManager _userManager;

        public Agent_EmpresaController()
        {

        }

        public Agent_EmpresaController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        private ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            set
            {
                _userManager = value;
            }
        }

        [Authorize(Roles = AccessPolicy.SuperAdministrators)]
        public ActionResult Index()
        {
            return View(db.Agent_Empresa.ToList());
        }

        [Authorize(Roles = AccessPolicy.SuperAdministrators)]
        public ActionResult Create()
        {
            Log.InfoFormat("EmpresaRegistro: formulario solicitado requestId={0}", HttpContext.Items["RequestLogId"]);
            return View();
        }

        /// <summary>
        /// Method to add th basic functionality
        /// </summary>
        /// <param name="company"></param>
        public void AddBasicFuctionality(ref Agent_Empresa company)
        {
            var idBaseCompany = Guid.Parse(ConfigurationManager.AppSettings["BaseCompany"]);
            var companyFunctionalityBase = db.CompanyFunctionality.Where(f => f.IdCompany == idBaseCompany).FirstOrDefault();
            Log.InfoFormat("EmpresaRegistro: empresa base funcionalidad encontrada={0} baseCompanyId={1} requestId={2}", companyFunctionalityBase != null, idBaseCompany, HttpContext.Items["RequestLogId"]);
            if (companyFunctionalityBase == null) throw new InvalidOperationException("La empresa base no tiene una funcionalidad asignada.");
            CompanyFunctionality functionality = new CompanyFunctionality();
            functionality.IdFunctionality = companyFunctionalityBase.IdFunctionality;
            functionality.IdCompany = company.IdCompany;
            functionality.Functionality = companyFunctionalityBase.Functionality;
            functionality.AgentEmpresa = company;
            db.CompanyFunctionality.Add(functionality);
            db.SaveChanges();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="company"></param>
        public void AddHorary(ref Agent_Empresa company)
        {
            var baseCompanyId = Guid.Parse(ConfigurationManager.AppSettings["BaseCompany"]);
            var horaryBaseCompany = db.Agent_GroupHorary.Where(f => f.IdCompany == baseCompanyId).ToList();
            Log.InfoFormat("EmpresaRegistro: empresa base horarios count={0} baseCompanyId={1} requestId={2}", horaryBaseCompany.Count, baseCompanyId, HttpContext.Items["RequestLogId"]);
            foreach(var horary in horaryBaseCompany)
            {
                var new_agent = new Agent_GroupHorary();
                new_agent.IdCompany = company.IdCompany;
                new_agent.Id_GroupHorary = Guid.NewGuid();
                new_agent.NameGroup = horary.NameGroup;
                db.Agent_GroupHorary.Add(new_agent);
                var horaryDetailBaseCompany = db.Agent_GroupHoraryDetail.Where(f => f.Id_GroupHorary == horary.Id_GroupHorary).ToList();
                foreach(var detail in horaryDetailBaseCompany)
                {
                    var new_horaryDetail = new Agent_GroupHoraryDetail();
                    new_horaryDetail.Id_GroupHoraryDetail = Guid.NewGuid();
                    new_horaryDetail.Id_GroupHorary = new_agent.Id_GroupHorary;
                    new_horaryDetail.Day = detail.Day;
                    new_horaryDetail.Agent_GroupHorary = new_agent;
                    new_horaryDetail.Type = detail.Type;
                    new_horaryDetail.HourUntil = detail.HourUntil;
                    new_horaryDetail.HourFrom = detail.HourFrom;
                    db.Agent_GroupHoraryDetail.Add(new_horaryDetail);
                }
            }
            db.SaveChanges();        
        }

        public void AddConfiguration(ref Agent_Empresa company)
        {
            var baseCompanyId = Guid.Parse(ConfigurationManager.AppSettings["BaseCompany"]);
            var template = db.Agent_Configuration.FirstOrDefault(f => f.IdCompany == baseCompanyId);
            Log.InfoFormat("EmpresaRegistro: empresa base configuración encontrada={0} baseCompanyId={1} requestId={2}", template != null, baseCompanyId, HttpContext.Items["RequestLogId"]);
            var configuration = new Agent_Configuration {
                Id_Configuration = Guid.NewGuid(),
                IdCompany = company.IdCompany,
                Agent_Empresa = company,
                DateCreation = DateTime.Now
            };
            if (template != null)
            {
                configuration.CaptureFrecuency = template.CaptureFrecuency;
                configuration.InactivityPeriod = template.InactivityPeriod;
                configuration.LocationFrecuency = template.LocationFrecuency;
                configuration.UploadFrecuency = template.UploadFrecuency;
            }
            else
            {
                // Preserve the model defaults for seconds; use a positive location interval.
                configuration.LocationFrecuency = 1;
                Log.WarnFormat("EmpresaRegistro: falta configuración base; se utilizarán valores predeterminados companyId={0} requestId={1}", company.IdCompany, HttpContext.Items["RequestLogId"]);
            }
            Log.InfoFormat("EmpresaRegistro: guardar configuración origen={0} companyId={1} inactividadSegundos={2} envioSegundos={3} capturasSegundos={4} ubicacionMinutos={5} requestId={6}",
                template == null ? "predeterminada" : "empresa base", company.IdCompany, configuration.InactivityPeriod,
                configuration.UploadFrecuency, configuration.CaptureFrecuency, configuration.LocationFrecuency, HttpContext.Items["RequestLogId"]);
            db.Agent_Configuration.Add(configuration);
            db.SaveChanges();
            Log.InfoFormat("EmpresaRegistro: configuración guardada configurationId={0} companyId={1} requestId={2}", configuration.Id_Configuration, company.IdCompany, HttpContext.Items["RequestLogId"]);
        }
        /// <summary>
        /// Method to add basic licence of 30 days from the day of creation of the company
        /// </summary>
        /// <param name="company"></param>
        public void AddBasicLicense(ref Agent_Empresa company)
        {
            var baseCompanyId = Guid.Parse(ConfigurationManager.AppSettings["BaseCompany"]);
            var licenseBaseCompany = db.License.Where(f => f.Agent_Empresa.IdCompany == baseCompanyId).FirstOrDefault();
            Log.InfoFormat("EmpresaRegistro: empresa base licencia encontrada={0} baseCompanyId={1} requestId={2}", licenseBaseCompany != null, baseCompanyId, HttpContext.Items["RequestLogId"]);
            if (licenseBaseCompany == null) throw new InvalidOperationException("La empresa base no tiene una licencia.");
            Log.InfoFormat("EmpresaRegistro: licencia copiada vencimiento={0:yyyy-MM-dd} vigente={1} requestId={2}", licenseBaseCompany.enddate, licenseBaseCompany.enddate >= DateTime.Today, HttpContext.Items["RequestLogId"]);
            License license = new License();
            license.IdLicense = Guid.NewGuid();
            license.enddate = licenseBaseCompany.enddate;
            license.Agent_Empresa = company;
            db.License.Add(license);
            db.SaveChanges();       
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AccessPolicy.SuperAdministrators)]
        public async Task<ActionResult> Create(Agent_Empresa agent_Empresa)
        {
            var requestId = Convert.ToString(HttpContext.Items["RequestLogId"]);
            if (string.IsNullOrWhiteSpace(requestId)) requestId = Guid.NewGuid().ToString("N");
            Log.InfoFormat("EmpresaRegistro: intento recibido valid={0} requestId={1}", ModelState.IsValid, requestId);
            if (!ModelState.IsValid)
            {
                foreach (var field in ModelState.Where(s => s.Value.Errors.Count > 0))
                    Log.WarnFormat("EmpresaRegistro: validación rechazada campo={0} errores={1} requestId={2}", field.Key, field.Value.Errors.Count, requestId);
                return View(agent_Empresa);
            }
            var stage = "preparar empresa";
            string createdUserId = null;
            try
            {
                agent_Empresa.IdCompany = Guid.NewGuid();
                Log.InfoFormat("EmpresaRegistro: empresa preparada companyId={0} requestId={1}", agent_Empresa.IdCompany, requestId);
                db.Agent_Empresa.Add(agent_Empresa);
                stage = "funcionalidad inicial";
                Log.InfoFormat("EmpresaRegistro: inicio etapa={0} requestId={1}", stage, requestId);
                AddBasicFuctionality(ref agent_Empresa);
                Log.InfoFormat("EmpresaRegistro: completada etapa={0} requestId={1}", stage, requestId);
                stage = "licencia inicial";
                Log.InfoFormat("EmpresaRegistro: inicio etapa={0} requestId={1}", stage, requestId);
                AddBasicLicense(ref agent_Empresa);
                Log.InfoFormat("EmpresaRegistro: completada etapa={0} requestId={1}", stage, requestId);
                stage = "horarios iniciales";
                Log.InfoFormat("EmpresaRegistro: inicio etapa={0} requestId={1}", stage, requestId);
                AddHorary(ref agent_Empresa);
                Log.InfoFormat("EmpresaRegistro: completada etapa={0} requestId={1}", stage, requestId);
                stage = "configuración inicial";
                Log.InfoFormat("EmpresaRegistro: inicio etapa={0} requestId={1}", stage, requestId);
                AddConfiguration(ref agent_Empresa);
                Log.InfoFormat("EmpresaRegistro: completada etapa={0} requestId={1}", stage, requestId);
                stage = "crear administrador";
                Log.InfoFormat("EmpresaRegistro: inicio etapa={0} requestId={1}", stage, requestId);
                var username = "Ad_" + agent_Empresa.Nombre.Replace(" ", "").Replace(".", "").ToLower();
                var user = new ApplicationUser { UserName = username, Email = agent_Empresa.Email, FirstName = agent_Empresa.Nombre, LastName = agent_Empresa.Rut };
                var password = System.Web.Security.Membership.GeneratePassword(8, 1);
                var result = await UserManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    Log.WarnFormat("EmpresaRegistro: Identity rechazó administrador errores={0} requestId={1}", string.Join("; ", result.Errors), requestId);
                    throw new InvalidOperationException("No se pudo crear el administrador: " + string.Join("; ", result.Errors));
                }
                createdUserId = user.Id;
                Log.InfoFormat("EmpresaRegistro: administrador creado userId={0} requestId={1}", user.Id, requestId);
                stage = "asignar rol Admin";
                Log.InfoFormat("EmpresaRegistro: inicio etapa={0} requestId={1}", stage, requestId);
                var role = db.Roles.SingleOrDefault(r => r.Name == "Admin");
                if (role == null) throw new InvalidOperationException("No existe el rol Admin.");
                var roleResult = await UserManager.AddToRoleAsync(user.Id, role.Name);
                if (!roleResult.Succeeded) throw new InvalidOperationException("No se pudo asignar el rol Admin: " + string.Join("; ", roleResult.Errors));
                Log.InfoFormat("EmpresaRegistro: completada etapa={0} requestId={1}", stage, requestId);
                stage = "preparar estado del administrador";
                var savedUser = db.Users.Find(user.Id);
                if (savedUser == null) throw new InvalidOperationException("No se encontró el administrador después de crearlo.");
                savedUser.EmailConfirmed = true;
                savedUser.LockoutEnabled = false;
                db.Entry(savedUser).State = EntityState.Modified;
                stage = "invitación por correo";
                bool sendInvitation = bool.Parse(ConfigurationManager.AppSettings["EmailNotification"]);
                Log.InfoFormat("EmpresaRegistro: inicio etapa={0} habilitada={1} requestId={2}", stage, sendInvitation, requestId);
                if (sendInvitation) await new EmailController().SendInvitation(new List<string> { savedUser.Email }, savedUser.Email, password);
                Log.InfoFormat("EmpresaRegistro: completada etapa={0} enviada={1} requestId={2}", stage, sendInvitation, requestId);
                stage = "guardar estado del administrador";
                await db.SaveChangesAsync();
                Log.InfoFormat("EmpresaRegistro: completada etapa={0} requestId={1}", stage, requestId);
                stage = "relacionar usuario y empresa";
                Log.InfoFormat("EmpresaRegistro: inicio etapa={0} companyId={1} userId={2} requestId={3}", stage, agent_Empresa.IdCompany, savedUser.Id, requestId);
                db.Agent_UserCompany.Add(new Agent_UserCompanies { idUser = Guid.Parse(savedUser.Id), IdCompany = agent_Empresa.IdCompany });
                await db.SaveChangesAsync();
                Log.InfoFormat("EmpresaRegistro: registro completado companyId={0} userId={1} requestId={2}", agent_Empresa.IdCompany, savedUser.Id, requestId);
                return RedirectToAction("Index", "Agent_Empresa");
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("EmpresaRegistro: fallo etapa={0} companyId={1} userId={2} requestId={3}", stage, agent_Empresa.IdCompany, createdUserId ?? "no creado", requestId), ex);
                var validationException = ex as System.Data.Entity.Validation.DbEntityValidationException;
                if (validationException != null)
                    foreach (var entity in validationException.EntityValidationErrors)
                        foreach (var error in entity.ValidationErrors)
                            Log.WarnFormat("EmpresaRegistro: validación SQL entidad={0} campo={1} mensaje={2} requestId={3}", entity.Entry.Entity.GetType().Name, error.PropertyName, error.ErrorMessage, requestId);
                try
                {
                    Log.InfoFormat("EmpresaRegistro: limpieza iniciada companyId={0} requestId={1}", agent_Empresa.IdCompany, requestId);
                    DeleteAllAditionsToCompany(agent_Empresa);
                    Log.InfoFormat("EmpresaRegistro: limpieza completada requestId={0}", requestId);
                }
                catch (Exception cleanupError)
                {
                    Log.Error(string.Format("EmpresaRegistro: limpieza falló; revisar registros parciales companyId={0} requestId={1}", agent_Empresa.IdCompany, requestId), cleanupError);
                }
                if (createdUserId != null)
                    Log.WarnFormat("EmpresaRegistro: revisar administrador creado antes del fallo; la limpieza actual no elimina cuentas Identity userId={0} requestId={1}", createdUserId, requestId);
                agent_Empresa.IdCompany = Guid.Empty;
                ModelState.Remove("IdCompany");
                ModelState.AddModelError("", "No se pudo registrar la empresa en la etapa «" + stage + "». Referencia para revisar los logs: " + requestId + ".");
                return View(agent_Empresa);
            }
        }
        /// <summary>
        /// Method to delete all aditions to company and the company itself
        /// </summary>
        /// <param name="company"></param>
        public void DeleteAllAditionsToCompany(Agent_Empresa company)
        {
            if (company != null)
            {
                var configuration = db.Agent_Configuration.Where(f => f.Agent_Empresa.IdCompany == company.IdCompany).FirstOrDefault();
                if (configuration != null)
                {
                    db.Agent_Configuration.Remove(configuration);
                }
                var licence = db.License.Where(f => f.Agent_Empresa.IdCompany == company.IdCompany).FirstOrDefault();
                if (licence != null)
                {
                    db.License.Remove(licence);
                }
                var functionalities = db.CompanyFunctionality.Where(f => f.IdCompany == company.IdCompany).ToList();
                if (functionalities.Any())
                {
                    db.CompanyFunctionality.RemoveRange(functionalities);
                }
                var horary = db.Agent_GroupHorary.Where(f => f.IdCompany == company.IdCompany).ToList();
                if (horary.Any())
                {
                    foreach (var hs in horary)
                    {
                        var horaryDetails = db.Agent_GroupHoraryDetail.Where(f => f.Id_GroupHorary == hs.Id_GroupHorary).ToList();
                        if (horaryDetails.Any())
                        {
                            db.Agent_GroupHoraryDetail.RemoveRange(horaryDetails);
                        }
                    }
                    db.Agent_GroupHorary.RemoveRange(horary);
                }
                db.Agent_Empresa.Remove(company);
                db.SaveChanges();
            }
        }

        public ActionResult Details(Guid? id)
        {
            if (!id.HasValue) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var company = db.Agent_Empresa.Find(id.Value);
            if (company == null) return HttpNotFound();
            return View(company);
        }

        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Agent_Empresa agent_Empresa = db.Agent_Empresa.Find(id);
            if (agent_Empresa == null)
            {
                return HttpNotFound();
            }
            ViewBag.status = agent_Empresa.status;
            return View(agent_Empresa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Agent_Empresa agent_Empresa)
        {
            if (ModelState.IsValid)
            {
                var existing = db.Agent_Empresa.Find(agent_Empresa.IdCompany);
                if (existing == null) return HttpNotFound();
                existing.Nombre = agent_Empresa.Nombre;
                existing.Direccion = agent_Empresa.Direccion;
                existing.Telefono = agent_Empresa.Telefono;
                existing.Email = agent_Empresa.Email;
                existing.Rut = agent_Empresa.Rut;
                existing.Key = agent_Empresa.Key;
                existing.status = agent_Empresa.string_status != "Inactive";
                existing.string_status = agent_Empresa.string_status;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(agent_Empresa);
        }
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Agent_Empresa agent_Empresa = db.Agent_Empresa.Find(id);
            if (agent_Empresa == null)
            {
                return HttpNotFound();
            }
            return View(agent_Empresa);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Agent_Empresa agent_Empresa = db.Agent_Empresa.Find(id);
            DeleteAllAditionsToCompany(agent_Empresa);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
