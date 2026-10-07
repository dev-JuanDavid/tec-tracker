using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Queue.DAL;
using Queue.Models;

namespace Queue.Controllers
{
    [Authorize(Roles = AccessPolicy.SuperAdministrators)]
    public class FunctionalityController : BaseController
    {
        private QueueContext db = new QueueContext();

        private void PopulateLitItemSelect()
        {
            //functionalidades actualmente en la base de datos
            var functionalities = db.Functionality.ToList();
            //Se crea la lista de con id y nombre de funcionalidad
            var selectList = functionalities.Where(f => f.Active == true).Select(f => new SelectListItem
            {
                Text = f.Name,
                Value = f.IdFunctionality.ToString()

            }).ToList();

            //
            ViewBag.Functionality = selectList;

    
        }

        // GET: Functionalities
        public ActionResult Index(Guid idCompany)
        {
            var company = db.Agent_Empresa.Find(idCompany);
            if (company == null) return HttpNotFound();
            ViewBag.CompanyName = company.Nombre;

            var querytableCompanyFunctionalities = db.CompanyFunctionality.AsQueryable();

            //lista funcionalidades de una determinada empresa 
            var functionalities = (from f in querytableCompanyFunctionalities
                                   where f.IdCompany == idCompany
                                   select f.Functionality).ToList();

            ViewBag.IdCompany = idCompany;

            return View(functionalities);
        }

        // GET: Functionalities/Details/5
        public ActionResult Details(Guid? id, Guid? idCompany)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Functionality functionality = db.Functionality.Find(id);
            if (functionality == null)
            {
                return HttpNotFound();
            }
            functionality.IdCompany = idCompany ?? Guid.Empty;
            return View(functionality);
        }

        // GET: Functionalities/Create
        public ActionResult Create(Guid idCompany)
        {
            PopulateLitItemSelect();

            ViewBag.IdCompany = idCompany;

            var company = db.Agent_Empresa.Find(idCompany);
            if (company == null) return HttpNotFound();
            return View(new Functionality { IdCompany = idCompany });
        }

       

        // POST: Functionalities/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( Functionality functionality)
        {
            PopulateLitItemSelect();
            ViewBag.IdCompany = functionality.IdCompany;
            if (db.Agent_Empresa.Find(functionality.IdCompany) == null) return HttpNotFound();
            if (!db.Functionality.Any(f => f.IdFunctionality == functionality.IdFunctionality && f.Active))
                ModelState.AddModelError("IdFunctionality", "Selecciona una funcionalidad activa del catálogo.");

            if (ModelState.IsValid)
            {
                var idCompany = (Guid)functionality.IdCompany;
                //buscar si la funcionalidad seleccionada ya se encuentra en la empresa
                var querytableCompanyFunctionalities = db.CompanyFunctionality.AsQueryable();
                
                //funcionalidades que tiene la empresa con id = IdEmpresa
                var functionalities = (from f in querytableCompanyFunctionalities
                                       where f.IdCompany == idCompany
                                       select f.Functionality).ToList();

                // busco que el id de la funcionalidad escogido el el dropdown no se encuentre entre las funcionalidades
                // de la empresa
                var searchFunctionality = functionalities
                    .Where(f => f.IdFunctionality.CompareTo(functionality.IdFunctionality) == 0);
                
                if (!searchFunctionality.Any())
                {
                    db.CompanyFunctionality.Add(new CompanyFunctionality()
                    {
                        IdCompany = idCompany,
                        IdFunctionality = functionality.IdFunctionality

                    });
                    db.SaveChanges();
                    Success("La funcionalidad se agregó a la empresa");
                    return RedirectToAction("Index", new {idCompany= idCompany });

                } else
                {
                    ModelState.AddModelError("IdFunctionality", "La funcionalidad ya está asignada a la empresa.");
  
                }

            }

            return View(functionality);
        }

        // GET: Functionalities/Edit/5
        public ActionResult Edit(Guid? id, Guid? idCompany)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Functionality functionality = db.Functionality.Find(id);
            if (functionality == null)
            {
                return HttpNotFound();
            }
            functionality.IdCompany = idCompany ?? Guid.Empty;
            return View(functionality);
        }

        // POST: Functionalities/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdFunctionality,Name,Active,IdCompany")] Functionality functionality)
        {
            var existing = db.Functionality.Find(functionality.IdFunctionality);
            if (existing == null || db.Agent_Empresa.Find(functionality.IdCompany) == null) return HttpNotFound();
            if (string.IsNullOrWhiteSpace(functionality.Name)) ModelState.AddModelError("Name", "Ingresa el nombre de la funcionalidad.");
            if (ModelState.IsValid)
            {
                existing.Name = functionality.Name.Trim();
                existing.Active = functionality.Active;
                db.SaveChanges();
                return RedirectToAction("Index", new { idCompany = functionality.IdCompany });
            }
            return View(functionality);
        }

        // GET: Functionalities/Delete/5
        public ActionResult Delete(Guid idCompany, Guid idFunctionality)
        {
            if (idCompany == Guid.Empty || idFunctionality == Guid.Empty)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            Functionality functionality = db.Functionality.Find(idFunctionality);
            
            if (functionality == null)
            {
                return HttpNotFound();
            }

            if (!db.CompanyFunctionality.Any(f => f.IdCompany == idCompany && f.IdFunctionality == idFunctionality)) return HttpNotFound();
            functionality.IdCompany = idCompany;
            
            return View(functionality);
        }

        // POST: Functionalities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Functionality functionality)
        {
            var idCompany = functionality.IdCompany;

            var idFunctionality = functionality.IdFunctionality;

            var companyFunctionalityToRemove = db.CompanyFunctionality.Find(idCompany, idFunctionality);

      

            if (companyFunctionalityToRemove != null )
            {
                db.CompanyFunctionality.Remove(companyFunctionalityToRemove);
                db.SaveChanges();
                Success("Se retiró la funcionalidad de la empresa");
                return RedirectToAction("Index", new { idCompany = idCompany });
            }

            return HttpNotFound();

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
