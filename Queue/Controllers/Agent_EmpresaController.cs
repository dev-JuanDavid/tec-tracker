using Microsoft.AspNet.Identity.Owin;
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

        [Authorize(Roles = "SAdmin")]
        public ActionResult Index()
        {
            return View(db.Agent_Empresa.ToList());
        }

        [Authorize(Roles = "SAdmin")]
        public ActionResult Create()
        {
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
            var configuratioBaseCompany = db.Agent_Configuration.Where(f => f.Agent_Empresa.IdCompany == baseCompanyId).FirstOrDefault();
            Agent_Configuration configuration = new Agent_Configuration();
            configuration.Id_Configuration = Guid.NewGuid();
            configuration.Agent_Empresa = company;
            configuration.CaptureFrecuency = configuratioBaseCompany.CaptureFrecuency;
            configuration.DateCreation = configuratioBaseCompany.DateCreation;
            configuration.InactivityPeriod = configuratioBaseCompany.InactivityPeriod;
            configuration.LocationFrecuency = configuratioBaseCompany.LocationFrecuency;
            configuration.UploadFrecuency = configuratioBaseCompany.UploadFrecuency;
            db.Agent_Configuration.Add(configuration);
            db.SaveChanges();
        }

        /// <summary>
        /// Method to add basic licence of 30 days from the day of creation of the company
        /// </summary>
        /// <param name="company"></param>
        public void AddBasicLicense(ref Agent_Empresa company)
        {
            var baseCompanyId = Guid.Parse(ConfigurationManager.AppSettings["BaseCompany"]);
            var licenseBaseCompany = db.License.Where(f => f.Agent_Empresa.IdCompany == baseCompanyId).FirstOrDefault();
            License license = new License();
            license.IdLicense = Guid.NewGuid();
            license.enddate = licenseBaseCompany.enddate;
            license.Agent_Empresa = company;
            db.License.Add(license);
            db.SaveChanges();       
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SAdmin")]
        public async Task<ActionResult> Create(Agent_Empresa agent_Empresa)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //variables iniciales
                    var CompanyId = Guid.NewGuid();
                    //Creacion de la empresa
                    agent_Empresa.IdCompany = CompanyId;
                    db.Agent_Empresa.Add(agent_Empresa);
                    //add Basic Functionality
                    AddBasicFuctionality(ref agent_Empresa);
                    //add Basic License
                    AddBasicLicense(ref agent_Empresa);
                    //add horary
                    AddHorary(ref agent_Empresa);
                    //add configuration
                    AddConfiguration(ref agent_Empresa);
                    //Crear Usuario Admnistrador a la empresa
                    var _username = String.Concat("Ad_", agent_Empresa.Nombre.Replace(" ", "").Replace(".", "").ToLower().ToString());
                    var user = new ApplicationUser { UserName = _username, Email = agent_Empresa.Email, FirstName = agent_Empresa.Nombre, LastName = agent_Empresa.Rut };
                    var Password = System.Web.Security.Membership.GeneratePassword(8, 1);
                    var result = await UserManager.CreateAsync(user, Password);
                    if (result.Succeeded)
                    {
                        var Role = db.Roles.Where(r => r.Name.Equals("Admin")).SingleOrDefault();
                        await UserManager.AddToRoleAsync(user.Id, Role.Name);
                        var oUser = db.Users.Find(user.Id);
                        oUser.EmailConfirmed = true;
                        oUser.LockoutEnabled = false;
                        db.Entry(oUser).State = EntityState.Modified;
                        var ec = new EmailController();
                        List<string> _mails = new List<string>();
                        _mails.Add(oUser.Email);
                        bool sendInvitation = bool.Parse(ConfigurationManager.AppSettings["EmailNotification"]);
                        if (sendInvitation) await ec.SendInvitation(_mails, oUser.Email, Password);
                        await db.SaveChangesAsync();
                        var relation = new Agent_UserCompanies()
                        {
                            idUser = Guid.Parse(oUser.Id),
                            IdCompany = CompanyId,
                        };
                        db.Agent_UserCompany.Add(relation);
                        db.SaveChanges();
                    }
                }
                //Rollback
                catch (Exception ex)
                {
                    DeleteAllAditionsToCompany(agent_Empresa);                   
                }
            }
            return RedirectToAction("Index", "Agent_Empresa");
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
                db.Entry(agent_Empresa).State = EntityState.Modified;
                if (agent_Empresa.string_status == "Inactive")
                    agent_Empresa.status = false;
                else
                    agent_Empresa.status = true;
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
