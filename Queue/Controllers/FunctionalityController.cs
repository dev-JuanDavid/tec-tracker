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

            var querytableCompanyFunctionalities = db.CompanyFunctionality.AsQueryable();

            //lista funcionalidades de una determinada empresa 
            var functionalities = (from f in querytableCompanyFunctionalities
                                   where f.IdCompany.CompareTo(idCompany) == 0
                                   select f.Functionality).ToList();

            ViewBag.IdCompany = idCompany;

            return View(functionalities);
        }

        // GET: Functionalities/Details/5
        public ActionResult Details(int? id)
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
            return View(functionality);
        }

        // GET: Functionalities/Create
        public ActionResult Create(Guid idCompany)
        {
            PopulateLitItemSelect();

            ViewBag.IdCompany = idCompany;

            return View();
        }

       

        // POST: Functionalities/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( Functionality functionality)
        {
            PopulateLitItemSelect();

            if (ModelState.IsValid)
            {
                var idCompany = (Guid)functionality.IdCompany;
                //buscar si la funcionalidad seleccionada ya se encuentra en la empresa
                var querytableCompanyFunctionalities = db.CompanyFunctionality.AsQueryable();
                
                //funcionalidades que tiene la empresa con id = IdEmpresa
                var functionalities = (from f in querytableCompanyFunctionalities
                                       where f.IdCompany.CompareTo(idCompany) == 0
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
                    Success("The functionality was added to the company");
                    return RedirectToAction("Index", new {idCompany= idCompany });

                } else
                {
                    Error("The functionality is already in the company", "");
  
                }

            }

            return View(functionality);
        }

        // GET: Functionalities/Edit/5
        public ActionResult Edit(int? id)
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
            return View(functionality);
        }

        // POST: Functionalities/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdFunctionality,Name,Active")] Functionality functionality)
        {
            if (ModelState.IsValid)
            {
                db.Entry(functionality).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(functionality);
        }

        // GET: Functionalities/Delete/5
        public ActionResult Delete(Guid idCompany, Guid idFunctionality)
        {
            if (idCompany == null || idFunctionality == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            Functionality functionality = db.Functionality.Find(idFunctionality);
            
            if (functionality == null)
            {
                return HttpNotFound();
            }

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
                Success("The functionality was deleted");
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
