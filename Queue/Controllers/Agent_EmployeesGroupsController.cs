using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Queue.DAL;
using Queue.DataBase;
using Queue.Models;

namespace Queue.Controllers
{
    [Authorize]
    public class Agent_EmployeesGroupsController : BaseController
    {
        private QueueContext db = new QueueContext();
        private IRepositorio repositorio = new Repositorio();

        // GET: Agent_EmployeesGroups
        public ActionResult Index()
        {
            var company = Request.RequestContext.HttpContext.Session["Company"].ToString();
            var guidCompany = Guid.Parse(company);

            return View(db.Agent_EmployeesGroups.Where(d => d.Agent_Empresa.IdCompany == guidCompany).ToList());
        }

        // GET: Agent_EmployeesGroups/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Agent_EmployeesGroups/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Agent_EmployeesGroups agent_EmployeesGroups)
        {
            if (ModelState.IsValid)
            {
                var company = Request.RequestContext.HttpContext.Session["Company"].ToString();
                var guidCompany = Guid.Parse(company);

                if (db.Agent_EmployeesGroups.Where(f => f.Agent_Empresa.IdCompany == guidCompany && f.Nombre == agent_EmployeesGroups.Nombre).Count() == 0)
                {
                    agent_EmployeesGroups.idemployeesGroup = Guid.NewGuid();
                    agent_EmployeesGroups.Agent_Empresa = db.Agent_Empresa.Where(d => d.IdCompany == guidCompany).SingleOrDefault();
                    db.Agent_EmployeesGroups.Add(agent_EmployeesGroups);
                    db.SaveChanges();
                    Success("Registro creado con exito");
                    return RedirectToAction("Index");
                }
                else
                    Warning("Grupo ya existe", "");
            }

            return View(agent_EmployeesGroups);
        }

        // GET: Agent_EmployeesGroups/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var company = Request.RequestContext.HttpContext.Session["Company"].ToString();
            var guidCompany = Guid.Parse(company);
            //Consulta el registro que voy a editar
            Agent_EmployeesGroups agent_EmployeesGroups = db.Agent_EmployeesGroups.Find(id);

           // Consultar relación por id del programa
            List<Agent_Employee> asigemp = repositorio.ListUsuarioArea(guidCompany, id.Value);
            //Consultar grupos
            List<Agent_Employee> empl = db.Agent_Employee.Where(s => s.IdCompany == guidCompany).ToList();

        

           
            //filtro los usuarios que actualmente se encuentran en el area de trabajo
            foreach(Agent_Employee employee in asigemp)
             {
                 empl = empl.Where(f => f.idEmployee != employee.idEmployee).ToList();
         
             }


            ViewBag.asigemp = asigemp;
            ViewBag.idemployee = new SelectList(empl, "idEmployee", "Nombre").ToList();
            agent_EmployeesGroups.Agent_EmployeeGroupsEmployee_list.AddRange(asigemp);
            if (agent_EmployeesGroups == null)
            {
                return HttpNotFound();
            }
            return View(agent_EmployeesGroups);
        }

        // POST: Agent_EmployeesGroups/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Agent_EmployeesGroups agent_EmployeesGroups)
        {
            if (ModelState.IsValid)
            {
                var company = Request.RequestContext.HttpContext.Session["Company"].ToString();
                var guidCompany = Guid.Parse(company);

                if (db.Agent_EmployeesGroups.Where(f => f.Agent_Empresa.IdCompany == guidCompany && f.Nombre == agent_EmployeesGroups.Nombre && f.idemployeesGroup != agent_EmployeesGroups.idemployeesGroup).Count() == 0)
                {
                   
                    db.Entry(agent_EmployeesGroups).State = EntityState.Modified;
                    db.SaveChanges();
                    Success("Registro editado con exito");
                    return RedirectToAction("Index");
                }
                else
                    Warning("Grupo ya existe", "");
            }
            return View(agent_EmployeesGroups);
        }
        [HttpPost]
        public ActionResult Addemployee(Guid idemployeesGroup, Guid idemployee)
        {


            WorkAreaEmployee workAreaEmployee = new WorkAreaEmployee();
            workAreaEmployee.idEmployee = idemployee;
            workAreaEmployee.IdWorkArea = idemployeesGroup;
            db.WorkAreaEmployee.Add(workAreaEmployee);



            db.SaveChanges();

            Success("Registro editado con exito");
            return RedirectToAction("Edit", new { id = idemployeesGroup });
        }

        public ActionResult Deleteemployee(Guid idEmployee, Guid idAgent_EmployeeGroupsEmployee)
        {
            
            WorkAreaEmployee workAreaEmployee = db.WorkAreaEmployee.Where(f => f.idEmployee == idEmployee && f.IdWorkArea == idAgent_EmployeeGroupsEmployee).FirstOrDefault();

            Guid idgroup = workAreaEmployee.IdWorkArea;

            if (workAreaEmployee != null)
            {
                db.WorkAreaEmployee.Remove(workAreaEmployee);
            }

            db.SaveChanges();

            Success("Registro eliminado con exito");
            return RedirectToAction("Edit", new { id = idgroup });
        }



        // GET: Agent_EmployeesGroups/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Agent_EmployeesGroups agent_EmployeesGroups = db.Agent_EmployeesGroups.Find(id);
            if (agent_EmployeesGroups == null)
            {
                return HttpNotFound();
            }
            return View(agent_EmployeesGroups);
        }

        // POST: Agent_EmployeesGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {

            if (db.Agent_EmployeeGroupsEmployee.Where(g => g.Agent_EmployeesGroups.idemployeesGroup == id).Count() == 0)
            {

                Agent_EmployeesGroups agent_EmployeesGroups = db.Agent_EmployeesGroups.Where(a => a.idemployeesGroup == id).SingleOrDefault();
                db.Agent_EmployeesGroups.Remove(agent_EmployeesGroups);
                db.SaveChanges();
                Success("Registro eliminado con exito");
                return RedirectToAction("Index");
            }
            else
            {
                Warning("El registro contiene datos asociados, no se puede eliminar", "");
            }
            return RedirectToAction("Delete", new { id = id });
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
