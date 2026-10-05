using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Queue.Controllers
{
    [Authorize]
    public class PcReportController : Controller
    {
        // GET: PcReport
        public ActionResult Index()
        {
            return View();
        }
    }
}