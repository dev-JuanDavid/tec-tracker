using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Queue.DAL;
using Queue.Models;
using Queue.Controllers;
using Queue.DataBase;

namespace Queue.Controllers
{
    [Authorize]
    public class Agent_ProgramClasificationController : BaseController
    {
        private QueueContext db = new QueueContext();

        public Guid idprogramclasification { get; set; } // Esta debe ser la propiedad ID
        public string name { get; set; }
        public string clasification { get; set; }
        public Agent_Empresa Agent_Empresa { get; set; }

        // GET: Agent_ProgramClasification
        public ActionResult Index()
        {
            Guid idcompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

            return View(db.Agent_ProgramClasification.Where(t => t.Agent_Empresa.IdCompany == idcompany).ToList());
        }


        // GET: Agent_ProgramClasification/Create
        public ActionResult Create()
        {
            Guid icompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
            OperationController opc = new OperationController();
            List<AutomaticTakeTimeModel> programs = opc.GetSoftWareClasification(icompany.ToString());

            List<SelectListItem> sli = new List<SelectListItem>();
            foreach (var i in programs.OrderBy(o => o.Application))
            {
                SelectListItem si = new SelectListItem();
                si.Text = i.Application;
                si.Value = i.Application;
                sli.Add(si);
            }

            ViewBag.name = new SelectList(sli, "Value", "Text");

            return View(new Agent_ProgramClasification());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Agent_ProgramClasification agent_ProgramClasification)
        {
            if (ModelState.IsValid)
            {
                Guid idcompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

                // Verificar si el programa con la misma clasificación ya existe
                bool exists = db.Agent_ProgramClasification.Any(apc =>
                    apc.name == agent_ProgramClasification.name &&
                    apc.clasification == agent_ProgramClasification.clasification &&
                    apc.Agent_Empresa.IdCompany == idcompany);

                if (exists)
                {
                    ModelState.AddModelError("", "Ya existe un programa con esta clasificación.");
                }
                else
                {
                    agent_ProgramClasification.idprogramclasification = Guid.NewGuid();
                    agent_ProgramClasification.Agent_Empresa = db.Agent_Empresa.Where(e => e.IdCompany == idcompany).SingleOrDefault();
                    db.Agent_ProgramClasification.Add(agent_ProgramClasification);

                    var program = new LicensePrograms();
                    program.idprogramclasification = agent_ProgramClasification.idprogramclasification;
                    program.isLicensed = agent_ProgramClasification.isLicensed;
                    program.licenseNumber = agent_ProgramClasification.licenseNumber;
                    program.creationDate = DateTime.Now;
                    db.LicensePrograms.Add(program);

                    db.SaveChanges();
                    Success("Registro Creado con Exito");
                    return RedirectToAction("Index");
                }
            }

            // Recargar la lista de programas y la clasificación en caso de error
            Guid icompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
            OperationController opc = new OperationController();
            List<AutomaticTakeTimeModel> programs = opc.GetSoftWareClasification(icompany.ToString());

            List<SelectListItem> sli = new List<SelectListItem>();
            foreach (var i in programs.OrderBy(o => o.Application))
            {
                SelectListItem si = new SelectListItem();
                si.Text = i.Application;
                si.Value = i.Application;
                if (agent_ProgramClasification.name == i.Application)
                    si.Selected = true;
                sli.Add(si);
            }

            ViewBag.name = new SelectList(sli, "Value", "Text");

            return View(agent_ProgramClasification);
        }


        // GET: Agent_ProgramClasification/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Agent_ProgramClasification agent_ProgramClasification = db.Agent_ProgramClasification.Find(id);
            if (agent_ProgramClasification == null)
            {
                return HttpNotFound();
            }

            OperationController opc = new OperationController();
            List<AutomaticTakeTimeModel> programs = opc.GetSoftWareClasification(Request.RequestContext.HttpContext.Session["Company"].ToString());

            var programItems = programs.Select(p => new SelectListItem
            {
                Text = p.Application,
                Value = p.Application,
                Selected = p.Application == agent_ProgramClasification.name // Marcar como seleccionado
            }).ToList();

            ViewBag.name = programItems; // Asignar la lista de selección al ViewBag

            ViewBag.clasification = agent_ProgramClasification.clasification;
            ViewBag.name_ = agent_ProgramClasification.name;
            ViewBag.isLicensed = agent_ProgramClasification.isLicensed;

            var license = db.LicensePrograms.FirstOrDefault(p => p.idprogramclasification == agent_ProgramClasification.idprogramclasification);
            if (license != null)
            {
                agent_ProgramClasification.isLicensed = license.isLicensed;
                agent_ProgramClasification.licenseNumber = license.licenseNumber;
            }

            return View(agent_ProgramClasification);
        }


        // POST: Agent_ProgramClasification/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Agent_ProgramClasification agent_ProgramClasification)
        {
            Guid idcompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
            if (ModelState.IsValid)
            {
                // Verificar si ya existe una clasificación igual para el mismo nombre
                bool exists = db.Agent_ProgramClasification.Any(apc =>
                    apc.name == agent_ProgramClasification.name &&
                    apc.clasification == agent_ProgramClasification.clasification &&
                    apc.idprogramclasification != agent_ProgramClasification.idprogramclasification &&
                    apc.Agent_Empresa.IdCompany == idcompany
                    );

                if (exists)
                {
                    ModelState.AddModelError("", "Ya existe un programa con la misma clasificación.");
                }
                else
                {
                    db.Entry(agent_ProgramClasification).State = EntityState.Modified;

                    var program = db.LicensePrograms.FirstOrDefault(p => p.idprogramclasification == agent_ProgramClasification.idprogramclasification);
                    if (program != null)
                    {
                        program.isLicensed = agent_ProgramClasification.isLicensed;
                        program.licenseNumber = agent_ProgramClasification.licenseNumber;
                        program.modifyDate = DateTime.Now;
                        db.Entry(program).State = EntityState.Modified;
                    }

                    db.SaveChanges();
                    Success("Registro Editado con Exito");
                    return RedirectToAction("Index");
                }
            }

            OperationController opc = new OperationController();

            List<AutomaticTakeTimeModel> programs = opc.GetSoftWareClasification(Request.RequestContext.HttpContext.Session["Company"].ToString());

            var programItems = programs.Select(p => new SelectListItem
            {
                Text = p.Application,
                Value = p.Application
            }).ToList();

            ViewBag.name = programItems;

            return View(agent_ProgramClasification);
        }

        // GET: Agent_ProgramClasification/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Agent_ProgramClasification agent_ProgramClasification = db.Agent_ProgramClasification.Find(id);
            if (agent_ProgramClasification == null)
            {
                return HttpNotFound();
            }
            return View(agent_ProgramClasification);
        }

        // POST: Agent_ProgramClasification/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Agent_ProgramClasification agent_ProgramClasification = db.Agent_ProgramClasification.Find(id);
            db.Agent_ProgramClasification.Remove(agent_ProgramClasification);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // Asignar Grupo
        public ActionResult AsignarGrupo(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

                var company = Request.RequestContext.HttpContext.Session["Company"].ToString();
                var guidCompany = Guid.Parse(company);

            Agent_ProgramClasification agent_ProgramClasification = db.Agent_ProgramClasification.Find(id);
                if (agent_ProgramClasification == null)
            {
                return HttpNotFound();
            }

                var programasRelacionados = db.Agent_ProgramClasification
                    .Where(p => p.name == agent_ProgramClasification.name && p.idprogramclasification != id)
                    .Select(p => p.idprogramclasification)
                    .ToList();

                var gruposAsignados = db.Agent_ClasificationGroups
                                        .Where(cg => cg.idprogramclasification == id)
                                        .Include(cg => cg.Agent_EmployeesGroups)
                                        .ToList();

                var gruposAsignadosAProgramasRelacionados = db.Agent_ClasificationGroups
                                                              .Where(cg => programasRelacionados.Contains(cg.idprogramclasification))
                                                              .ToList();

                var grupos = db.Agent_EmployeesGroups
                               .Where(g => g.Agent_Empresa.IdCompany == guidCompany)
                               .ToList();

                grupos = grupos.Where(g => !gruposAsignados.Any(ga => ga.idemployeesGroup == g.idemployeesGroup) &&
                                           !gruposAsignadosAProgramasRelacionados.Any(gr => gr.idemployeesGroup == g.idemployeesGroup))
                               .ToList();

            ViewBag.nombrePrograma = agent_ProgramClasification.name;
            ViewBag.asigemp = gruposAsignados;
            ViewBag.idemployeesGroup = new SelectList(grupos, "idemployeesGroup", "Nombre").ToList();

            ViewBag.clasification = agent_ProgramClasification.clasification;

            return View(agent_ProgramClasification);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddGrupo(Guid idprogramclasification, Guid idemployeesGroup, int clasification)
        {
            Guid idcompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
            // Obtener el programa actual
            var program = db.Agent_ProgramClasification.Find(idprogramclasification);
                if (program == null)
            {
                ModelState.AddModelError("", "El programa no existe.");
                return RedirectToAction("AsignarGrupo", new { id = idprogramclasification });
            }

            // Verificar si el grupo ya está asignado a esta clasificación
                var existingGroup = db.Agent_ClasificationGroups
                                      .FirstOrDefault(g => g.idemployeesGroup == idemployeesGroup &&
                                                           g.idprogramclasification == idprogramclasification);

                if (existingGroup != null)
            {
                ModelState.AddModelError("", "El grupo ya está asignado a esta clasificación.");
            }

            // Verificar si el grupo está asignado a otra clasificación del mismo programa
                var assignedToSameProgram = db.Agent_ClasificationGroups
                                              .Any(g => g.idemployeesGroup == idemployeesGroup &&
                                                        g.Agent_ProgramClasifications.name == program.name);

                if (assignedToSameProgram)
            {
                ModelState.AddModelError("", "El grupo ya está asignado a otra clasificación del mismo programa.");
            }

            // Si hay errores en el modelo, redirige de nuevo
                if (!ModelState.IsValid)
            {
                return RedirectToAction("AsignarGrupo", new { id = idprogramclasification });
            }

            // Agrega el grupo si no hay conflictos
                var clasificationGroup = new Agent_ClasificationGroup
                {
                    Id = Guid.NewGuid(),
                    idemployeesGroup = idemployeesGroup,
                    idprogramclasification = idprogramclasification,
                    clasification = clasification
                };

                db.Agent_ClasificationGroups.Add(clasificationGroup);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Grupo Agregado con Exito.";
                return RedirectToAction("AsignarGrupo", new { id = idprogramclasification });
        }

        // Delete Program - Group control ClasificationGroup 
        public ActionResult DeleteProgramGroup(Guid Id, Guid idprogramclasification)
        {
            Agent_ClasificationGroup ClasificationGroup = db.Agent_ClasificationGroups.Find(Id);

                if (ClasificationGroup != null)
            {
                db.Agent_ClasificationGroups.Remove(ClasificationGroup);
                db.SaveChanges();
                Success("Registro Eliminado con Exito.");
            }

            return RedirectToAction("AsignarGrupo", new { id = idprogramclasification });
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
