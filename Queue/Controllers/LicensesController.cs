using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Queue.DAL;
using Queue.Models;

namespace Queue.Controllers
{
    [Authorize]
    public class LicensesController : BaseController
    {
        private QueueContext db = new QueueContext();

        // GET: Licenses
        public ActionResult Index(Guid idempresa)
        {
            ViewBag.idempresa = idempresa;
            if (idempresa != null)
            {
                return View(db.License.Where(j => j.Agent_Empresa.IdCompany == idempresa).OrderBy(o => o.enddate).ToList());
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: Licenses/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            License license = db.License.Find(id);
            if (license == null)
            {
                return HttpNotFound();
            }
            return View(license);
        }

        // GET: Licenses/Create
        public ActionResult Create(Guid idempresa)
        {
            License lc = new License();
            lc.idempresa = idempresa;

            ViewBag.idempresa = idempresa;
            return View(lc);
        }

        // POST: Licenses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(License license)
        {
            if (ModelState.IsValid)
            {
                license.IdLicense = Guid.NewGuid();
                license.Agent_Empresa = db.Agent_Empresa.Where(g => g.IdCompany == license.idempresa).SingleOrDefault();
                db.License.Add(license);
                db.SaveChanges();
                return RedirectToAction("Index", new { idempresa = license.idempresa });
            }

            return View(license);
        }

        // GET: Licenses/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            License license = db.License.Find(id);
            if (license == null)
            {
                return HttpNotFound();
            }
            ViewBag.endate = license.enddate;
            return View(license);
        }

        // POST: Licenses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(License license)
        {
            if (ModelState.IsValid)
            {
                var newLicense = new License();
                newLicense.IdLicense = license.IdLicense;
                newLicense.Agent_Empresa = db.License.Find(license.IdLicense).Agent_Empresa;
                newLicense.enddate = license.enddate;
                newLicense.idempresa = newLicense.Agent_Empresa.IdCompany;

              //  db.License.Remove(license);

                db.License.AddOrUpdate(newLicense);



                db.SaveChanges();
                
                
                return RedirectToAction("Index", new { idempresa = newLicense.idempresa});
            }
            return View(license);
        }

        // GET: Licenses/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            License license = db.License.Find(id);
            if (license == null)
            {
                return HttpNotFound();
            }
            return View(license);
        }

        // POST: Licenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            License license = db.License.Find(id);
            db.License.Remove(license);
            db.SaveChanges();
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
