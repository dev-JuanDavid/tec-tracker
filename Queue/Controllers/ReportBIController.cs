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
    public class ReportBIController : BaseController
    {
        private QueueContext db = new QueueContext();

        // GET: Functionalities
        public ActionResult Index()
        {

            //var querytableCompanyFunctionalities = db.CompanyFunctionality.AsQueryable();

            ////lista funcionalidades de una determinada empresa 
            //var functionalities = (from f in querytableCompanyFunctionalities
            //                       where f.IdCompany.CompareTo(idCompany) == 0
            //                       select f.Functionality).ToList();

            //ViewBag.IdCompany = idCompany;

            return View();
        }

        // GET: Functionalities/Details/5
    }    
}
