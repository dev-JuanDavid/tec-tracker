using Queue.App_Start;
using Queue.DAL;
using Queue.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System.Data;
using System.Collections;
using System.Reflection;
using System.Globalization;
using System.Numerics;
using Queue.DataBase;
using System.Data.Entity.Core.Common.CommandTrees;

namespace Queue.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private QueueContext db = new QueueContext();

        private IRepositorio _repositorio = new Repositorio();

        int DecimalPlaces = 4;
        [SessionAuthorize]
        public ActionResult Index(DashBoardStats dsb)
        {
            DashBoardStats dasb = new DashBoardStats();
            Guid company = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

            if (dsb.DateFrom.Year <= 1900)
                dsb.DateFrom = DateTime.Today;

            if (dsb.DateTo.Year <= 1900)
                dsb.DateTo = DateTime.Today;


            OperationController opc = new OperationController();
            List<BasicStatsDashboard> data = opc.GetDataForDashBoard(company.ToString(), dsb.DateFrom, dsb.DateTo, dsb.ddlUsers, dsb.idgroup);

            //cuadritos de resumen
            dasb.resume = GetResume(data);
            //app mas usadas
            dasb.graph = GetAppResume(data);
            //resumen por usuario
            dasb.DataPerUser = GetTimePerUser(data);


            if (dsb.idgroup != null && dsb.idgroup != Guid.Empty)
            {
                EmployeeSelectList(dsb.idgroup);
            }

          //  ViewBag.ListUser = db.Agent_Employee.Where(u => u.IdCompany == company).Select(x => new SelectListItem() { Text = x.Usuario, Value = x.Usuario }).ToList();

            List<SelectListItem> sli = CreateList(db.Agent_EmployeesGroups.Where(c => c.Agent_Empresa.IdCompany == company).ToList(), "idemployeesGroup", "Nombre");
            sli.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.idgroup = sli;

            License licence_ = db.License.Where(c => c.Agent_Empresa.IdCompany == company && c.enddate >= DateTime.Today).SingleOrDefault();

            double restdays = (licence_.enddate - DateTime.Today).TotalDays;

            if (restdays <= 10)
            {
                ViewBag.restdaysmsg = "Quedan " + restdays + " días para el vencimiento de su licencia, comuniquese con soporte para la renovación";
                ViewBag.restdays = restdays;
            }
            return View(dasb);
        }


        public SelectList EmployeeSelectList(Guid idGroupEmployee)
        {
            Guid company = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
            var QueryUserbyArea = _repositorio.ListUsuarioArea(company, idGroupEmployee);
            var usersList = new SelectList(QueryUserbyArea, "Usuario", "Nombre");
            ViewData["UserList"] = usersList;
            return usersList;

        }


        [HttpPost]
        public JsonResult GetUserByArea(Guid idGroupEmployee)
        {
            try
            {
                var usersList = EmployeeSelectList(idGroupEmployee);
                return Json(usersList);
            }
            catch (Exception e)
            {
                return Json(e);
            }
        }




        private Resume GetResume(List<BasicStatsDashboard> data)
        {
            Resume rs = new Resume();

            foreach (var k in data.GroupBy(g => g.Clasification))
            {
                decimal total = 0;
                total = decimal.Parse(data.Where(f => f.Clasification == k.Key).Sum(b => b.Time).ToString());
                switch (k.Key)
                {
                    case 0:
                        rs.UnclasifyTime = total;
                        break;
                    case 1:
                        rs.ProductiveTime = total;
                        break;
                    case 2:
                        rs.ImproductiveTime = total;
                        break;
                    case 3:
                        rs.NeutralTime = total;
                        break;
                    default:
                        break;
                }
            }

            decimal totalactivity = rs.Total;
            if (totalactivity > 0)
            {
                rs.ProductiveTime = Math.Round((rs.ProductiveTime * 100) / totalactivity, 2);
                rs.ImproductiveTime = Math.Round((rs.ImproductiveTime * 100) / totalactivity, 2);
                rs.NeutralTime = Math.Round((rs.NeutralTime * 100) / totalactivity, 2);
                rs.UnclasifyTime = Math.Round((rs.UnclasifyTime * 100) / totalactivity, 2);
            }
            return rs;
        }
        public List<GraphData> GetAppResume(List<BasicStatsDashboard> data)
        {
            List<AppResume> lrs = new List<AppResume>();
            List<GraphData> lgd = new List<GraphData>();
            AppResume rs;
            DataTable dtdata = new DataTable();

            foreach (var k in data.GroupBy(g => g.Application))
            {
                rs = new AppResume();
                rs.appname = k.Key;                
                decimal timesum = Math.Round((decimal)data.Where(f => f.Application == k.Key).Sum(b => b.Time).Value,4);
                
                rs.Time = timesum;
                lrs.Add(rs);
            }

            decimal totalactivity = lrs.Select(t => t.Time).Sum();
            string rows_ = string.Empty;
            Guid company = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
            List<Agent_ProgramClasification> ac = db.Agent_ProgramClasification.Where(a => a.Agent_Empresa.IdCompany == company).ToList();

            foreach (var j in lrs)
            {
                GraphData gd = new GraphData();
                gd.element = j.appname;
                gd.decimalvalue = Math.Round((j.Time * 100) / totalactivity, 2);
                gd.value = gd.decimalvalue.ToString().Replace(",", ".") + "%";

                int clasification = ac.Where(p => p.name == j.appname).Select(f => f.clasification).SingleOrDefault();
                switch (clasification)
                {
                    case 0:
                        gd.color = "#2E3332";
                        break;
                    case 1:
                        gd.color = "#55CBCD";
                        break;
                    case 2:
                        gd.color = "#FF968A";
                        break;
                    case 3:
                        gd.color = "#96B3C2";
                        break;
                    default:
                        break;
                }

                lgd.Add(gd);
            }

            return lgd.OrderByDescending(o => o.decimalvalue).Take(5).ToList();
        }

        public List<BasicStatsDashboard> GetTimePerUser(List<BasicStatsDashboard> data)
        {
            BasicStatsDashboard rs;
            List<BasicStatsDashboard> lrs = new List<BasicStatsDashboard>();
            foreach (var j in data.GroupBy(h => h.User))
            {
                rs = new BasicStatsDashboard();
                rs.User = j.Key;

                foreach (var k in data.Where(d => d.User == j.Key).GroupBy(g => g.Clasification))
                {
                    string di = data.Where(f => f.Clasification == k.Key && f.User == j.Key).OrderBy(t => t.Date).Select(g => g.Date).FirstOrDefault();
                    string df = data.Where(f => f.Clasification == k.Key && f.User == j.Key).OrderByDescending(t => t.Date).Select(g => g.Date).FirstOrDefault();

                    double total = 0;
                    total = double.Parse(data.Where(f => f.Clasification == k.Key && f.User == j.Key).Sum(b => b.Time).ToString());
                    TimeSpan time = TimeSpan.FromSeconds(total);
                    string str = time.ToString(@"hh\:mm\:ss\:fff");

                    if (total > 0)
                    {
                        //para sacar cantidad de minutos
                        total = total / 60;
                        //para sacar las horas
                        total = total / 60;
                    }
                    switch (k.Key)
                    {
                        case 0:
                            rs.UnclasifyTime = total;
                            break;
                        case 1:
                            rs.ProductiveTime = total;
                            break;
                        case 2:
                            rs.ImproductiveTime = total;
                            break;
                        case 3:
                            rs.NeutralTime = total;
                            break;
                        default:
                            break;
                    }
                }

                lrs.Add(rs);
            }

            return lrs;
        }


        public static List<SelectListItem> CreateList(IEnumerable list, string dataValueField, string dataTextField, object selectedValue = null)
        {
            List<SelectListItem> sli = new List<SelectListItem>();
            SelectListItem sl;
            foreach (var i in list)
            {
                sl = new SelectListItem();
                foreach (PropertyInfo p in i.GetType().GetProperties())
                {
                    if (p.Name == dataTextField)
                        sl.Text = p.GetValue(i).ToString();

                    if (p.Name == dataValueField)
                    {
                        sl.Value = p.GetValue(i).ToString();
                        if (selectedValue != null)
                            if (sl.Value == selectedValue.ToString())
                                sl.Selected = true;
                    }
                }
                sli.Add(sl);
            }
            return sli;
        }

        [SessionAuthorize]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        [SessionAuthorize]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        public BigInteger GetFrom(decimal value)
        {
            string strValue = HasDecimalPlaces(value) ? ConvertAValueWithDecimalPlaces(value) : ConvertARoundedValue(value);
            return BigInteger.Parse(strValue);
        }

        private static bool HasDecimalPlaces(decimal value)
        {
            return !Math.Round(value).Equals(value) || value.ToString(CultureInfo.InvariantCulture).Contains(".");
        }

        private string ConvertAValueWithDecimalPlaces(decimal value)
        {
            var commaLeftRight = value.ToString(CultureInfo.InvariantCulture).Split('.');
            return commaLeftRight[0] + commaLeftRight[1].PadRight(DecimalPlaces, '0').Substring(0, DecimalPlaces);
        }

        private string ConvertARoundedValue(decimal value)
        {
            return value.ToString(CultureInfo.InvariantCulture) + new string('0', DecimalPlaces);
        }
    }
}