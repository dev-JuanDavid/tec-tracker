using log4net;
using Queue.DAL;
using Queue.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Queue.Common.CommonEnum;

namespace Queue.Controllers
{
    public class LocationController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private QueueContext db = new QueueContext();
        // GET: Location
        public ActionResult Index(string companykey, string userName)
        {
            ViewBag.Companykey = companykey;
            ViewBag.UserName = userName;
            return View();
        }

        [HttpPost]
        public void PostLocation(string companykey, string UserName, decimal Longitude, decimal Latitude)
        {
            try
            {
                var empoyee = db.Agent_Employee.Where(r => r.Usuario == UserName).FirstOrDefault();

                if (empoyee == null)
                {
                    _log.Error(Newtonsoft.Json.JsonConvert.SerializeObject(new DebugLogModel()
                    {
                        Method = "Location",
                        Message = $"El Empleado con nombre de usuario {UserName} no existe.",
                    }));

                    return;
                }

                //Actualiza Ubicación
                empoyee.Latitud = Latitude;
                empoyee.Longitud = Longitude;
                db.SaveChanges();

                //Inserta Historico
                var newLocation = new UserLocation
                {
                    Id = Guid.NewGuid(),
                    IdEmployee = empoyee.idEmployee,
                    IdCompany = empoyee.IdCompany,
                    Latitude = Latitude,
                    Longitude = Longitude,
                    Fecha = DateTime.UtcNow.AddHours(-5)
                };
                db.UserLocation.Add(newLocation);
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                _log.Error(Newtonsoft.Json.JsonConvert.SerializeObject(new DebugLogModel()
                {
                    Method = "Location",
                    Message = $"Error al momento de guardar la localización del usuario. {UserName}"
                }), ex);
            }
        }

    }
}