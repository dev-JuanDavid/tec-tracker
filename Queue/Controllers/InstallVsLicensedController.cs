using Queue.DAL;
using Queue.DataBase;
using Queue.Models;
using Queue.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Queue.Controllers
{
    public class InstallVsLicensedController : Controller
    {
        private QueueContext db = new QueueContext();
        private IRepositorio _repositorio;

        public InstallVsLicensedController()
        {
            MongoHelper.ConnectToMongoService();
            _repositorio = new Repositorio();
        }

        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                InstalledVsLicensedViewModel datos = new InstalledVsLicensedViewModel();
                Guid company = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

                var allWorkAreaList = _repositorio.ListGroups(company);
                ViewData["GroupList"] = new SelectList(allWorkAreaList, "idEmployeesGroup", "Nombre");
                datos.activities = new List<ProgramsLicensedActivityViewModel>();
                return View(datos);
            }
            catch (Exception ex)
            {
                return View();
            }
        }

      
        

        public ActionResult Index(InstalledVsLicensedViewModel activity)
        {
            var company = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

            if (!ModelState.IsValid)
            {
                var allWorkArea = _repositorio.ListGroups(company);
                ViewData["GroupList"] = new SelectList(allWorkArea, "idEmployeesGroup", "Nombre", activity.IdEmployeesGroup);

                var UserbyArea = _repositorio.ListUsuarioArea(company, activity.IdEmployeesGroup);
                var UsersList = new SelectList(UserbyArea, "Usuario", "Nombre", activity.idEmployee);
                ViewData["UserList"] = UsersList;

                activity.activities = new List<ProgramsLicensedActivityViewModel>();

                return View(activity);
            }


            var consultas = _repositorio.GetDataForInstalledVsLicensed(company.ToString(), activity.from, activity.to, activity.idEmployee, activity.IdEmployeesGroup.ToString());

            activity.activities = consultas.OrderByDescending(o => o.time).ToList();

            var allWorkAreaList = _repositorio.ListGroups(company);
            ViewData["GroupList"] = new SelectList(allWorkAreaList, "idEmployeesGroup", "Nombre", activity.IdEmployeesGroup);

            var QueryUserbyArea = _repositorio.ListUsuarioArea(company, activity.IdEmployeesGroup);
            var usersList = new SelectList(QueryUserbyArea, "Usuario", "Nombre", activity.idEmployee);
            ViewData["UserList"] = usersList;

            return View(activity);
        }

        [HttpPost]
        public JsonResult GetUserByArea(Guid idWorkArea)
        {
            try
            {

                Guid company = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
                var QueryUserbyArea = _repositorio.ListUsuarioArea(company, idWorkArea);
                var usersList = new SelectList(QueryUserbyArea, "Usuario", "Nombre");
                ViewData["UserList"] = QueryUserbyArea;
                return Json(usersList);
            }
            catch (Exception e)
            {
                return Json(e);
            }
        }


    }
}