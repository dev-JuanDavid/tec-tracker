using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Queue.DAL;
using Queue.Models;

namespace Queue.Controllers
{
    [Authorize(Roles = AccessPolicy.SuperAdministrators)]
    public class LicensesController : BaseController
    {
        private readonly QueueContext db = new QueueContext();
        public ActionResult Index(Guid idempresa)
        {
            var company = db.Agent_Empresa.Find(idempresa);
            if (company == null) return HttpNotFound();
            ViewBag.idempresa = company.IdCompany;
            ViewBag.CompanyName = company.Nombre;
            return View(db.License.Where(l => l.Agent_Empresa.IdCompany == idempresa).OrderByDescending(l => l.enddate).ToList());
        }
        public ActionResult Details(Guid? id) { return FindView(id); }
        public ActionResult Delete(Guid? id) { return FindView(id); }
        public ActionResult Edit(Guid? id) { return FindView(id); }
        private ActionResult FindView(Guid? id)
        {
            if (!id.HasValue) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var record = db.License.Find(id.Value);
            if (record == null) return HttpNotFound();
            record.idempresa = record.Agent_Empresa.IdCompany;
            return View(record);
        }
        public ActionResult Create(Guid idempresa)
        {
            var company = db.Agent_Empresa.Find(idempresa);
            if (company == null) return HttpNotFound();
            return View(new License { idempresa = idempresa, Agent_Empresa = company, enddate = DateTime.Today.AddDays(30) });
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(License record)
        {
            record.Agent_Empresa = db.Agent_Empresa.Find(record.idempresa);
            if (record.Agent_Empresa == null) return HttpNotFound();
            ValidateDate(record);
            if (!ModelState.IsValid) return View(record);
            record.IdLicense = Guid.NewGuid();
            record.enddate = record.enddate.Date;
            db.License.Add(record);
            db.SaveChanges();
            return RedirectToAction("Index", new { idempresa = record.idempresa });
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(License record)
        {
            var existing = db.License.Find(record.IdLicense);
            if (existing == null) return HttpNotFound();
            record.Agent_Empresa = existing.Agent_Empresa;
            record.idempresa = existing.Agent_Empresa.IdCompany;
            ValidateDate(record);
            if (!ModelState.IsValid) return View(record);
            existing.enddate = record.enddate.Date;
            db.SaveChanges();
            return RedirectToAction("Index", new { idempresa = record.idempresa });
        }
        private void ValidateDate(License record)
        {
            if (record.enddate.Year < 1900) ModelState.AddModelError("enddate", "Selecciona una fecha válida de vencimiento.");
        }
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            var record = db.License.Find(id);
            if (record == null) return HttpNotFound();
            var company = record.Agent_Empresa.IdCompany;
            db.License.Remove(record);
            db.SaveChanges();
            return RedirectToAction("Index", new { idempresa = company });
        }
        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
