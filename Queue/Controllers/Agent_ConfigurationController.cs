using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Queue.DAL;
using Queue.Models;

namespace Queue.Controllers
{
    [Authorize(Roles = AccessPolicy.SuperAdministrators)]
    public class Agent_ConfigurationController : Controller
    {
        private readonly QueueContext db = new QueueContext();
        public ActionResult Index(Guid? idCompany)
        {
            ViewBag.IdCompany = idCompany;
            var records = db.Agent_Configuration.AsQueryable();
            if (idCompany.HasValue) records = records.Where(c => c.IdCompany == idCompany);
            return View(records.ToList());
        }
        public ActionResult Details(Guid? id)
        {
            if (!id.HasValue) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var record = db.Agent_Configuration.Find(id.Value);
            if (record == null) return HttpNotFound();
            return View(record);
        }
        public ActionResult Create(Guid id)
        {
            var company = db.Agent_Empresa.Find(id);
            if (company == null) return HttpNotFound();
            var record = db.Agent_Configuration.FirstOrDefault(c => c.IdCompany == id);
            if (record != null) return RedirectToAction("Edit", new { id = record.Id_Configuration });
            return View(new Agent_Configuration { IdCompany = id, Agent_Empresa = company, DateCreation = DateTime.Now });
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Agent_Configuration record)
        {
            record.Agent_Empresa = record.IdCompany.HasValue ? db.Agent_Empresa.Find(record.IdCompany.Value) : null;
            if (record.Agent_Empresa == null) return HttpNotFound();
            if (db.Agent_Configuration.Any(c => c.IdCompany == record.IdCompany))
                ModelState.AddModelError("", "La empresa ya tiene una configuración. Regresa a empresas para editarla.");
            if (!ModelState.IsValid) return View(record);
            record.Id_Configuration = Guid.NewGuid();
            record.DateCreation = DateTime.Now;
            db.Agent_Configuration.Add(record);
            db.SaveChanges();
            return RedirectToAction("Details", "Agent_Empresa", new { id = record.IdCompany });
        }
        public ActionResult Edit(Guid? id)
        {
            if (!id.HasValue) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var record = db.Agent_Configuration.Find(id.Value);
            if (record == null) return HttpNotFound();
            return View(record);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Agent_Configuration record)
        {
            var existing = db.Agent_Configuration.Find(record.Id_Configuration);
            if (existing == null) return HttpNotFound();
            record.IdCompany = existing.IdCompany;
            record.Agent_Empresa = existing.Agent_Empresa;
            record.DateCreation = existing.DateCreation;
            if (!ModelState.IsValid) return View(record);
            existing.InactivityPeriod = record.InactivityPeriod;
            existing.UploadFrecuency = record.UploadFrecuency;
            existing.CaptureFrecuency = record.CaptureFrecuency;
            existing.LocationFrecuency = record.LocationFrecuency;
            db.SaveChanges();
            return RedirectToAction("Details", "Agent_Empresa", new { id = existing.IdCompany });
        }
        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
