using MongoDB.Driver;
using Queue.DAL;
using Queue.Models;
using Queue.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Queue.Controllers
{
    [Authorize(Roles = AccessPolicy.Administrators)]
    public class ReportTestDataController : Controller
    {
        private static readonly Guid EmployeeId = new Guid("FE78B48F-4400-4BC5-A16A-5CEA145AB7B3");
        private static readonly Guid CompanyId = new Guid("CF1EA8C5-D581-44D2-8E8C-0BD0137CA334");
        private const string TestPc = "TEC-PRUEBA-FE78B48F";
        private readonly QueueContext db = new QueueContext();

        private Agent_Employee GetEmployee()
        {
            Guid sessionCompany;
            if (!Request.IsLocal || !Guid.TryParse(Convert.ToString(Session["Company"]), out sessionCompany) || sessionCompany != CompanyId)
                throw new InvalidOperationException("Esta herramienta solo está disponible desde localhost, dentro de la empresa de prueba indicada.");
            var employee = db.Agent_Employee.SingleOrDefault(e => e.idEmployee == EmployeeId && e.IdCompany == CompanyId);
            if (employee == null || string.IsNullOrWhiteSpace(employee.Usuario))
                throw new InvalidOperationException("No se encontró el empleado en esta empresa o no tiene un usuario asociado.");
            return employee;
        }

        [HttpGet]
        public ActionResult Index()
        {
            try { ViewBag.EmployeeUser = GetEmployee().Usuario; }
            catch (InvalidOperationException ex) { return new HttpStatusCodeResult(403, ex.Message); }
            return View(DateTime.Today);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Load(DateTime? date)
        {
            try
            {
                var employee = GetEmployee();
                ViewBag.EmployeeUser = employee.Usuario;
                if (!date.HasValue || date.Value.Year < 2000 || date.Value.Year > 2100)
                {
                    ModelState.AddModelError("date", "Selecciona una fecha válida entre los años 2000 y 2100.");
                    return View("Index", date ?? DateTime.Today);
                }
                MongoHelper.ConnectToMongoService();
                var company = CompanyId.ToString();
                var activity = MongoHelper.database.GetCollection<TrakerBase>("TrackerTime");
                var software = MongoHelper.database.GetCollection<InstalledProgramsViewModel>("Software");
                var hardware = MongoHelper.database.GetCollection<InstalledHardwareViewModel>("Hardware");
                var classifications = db.Agent_ProgramClasification.Where(p => p.Agent_Empresa.IdCompany == CompanyId).OrderBy(p => p.idprogramclasification).ToList();
                classifications = classifications.Where(p => !string.IsNullOrWhiteSpace(p.name)).GroupBy(p => p.name, StringComparer.OrdinalIgnoreCase).Select(g => g.First()).ToList();
                var applications = Enumerable.Range(1, 3).Select(type => classifications.FirstOrDefault(p => p.clasification == type && !string.IsNullOrWhiteSpace(p.name))).ToList();
                var missing = applications.Any(p => p == null);
                var names = applications.Select((p, i) => p == null ? "PRUEBA - Programa " + (i + 1) : p.name).ToArray();
                // Stable identifiers make a retry replace the same sample, even after a partial failure.
                for (var i = 0; i < 24; i++)
                {
                    var focus = DateTime.SpecifyKind(date.Value.Date.AddHours(8).AddMinutes(i * 5), DateTimeKind.Local);
                    var record = new TrakerBase {
                        _id = SampleId(i), IdEmpresa = company, UserName = employee.Usuario, Pc = TestPc,
                        Ip = "127.0.0.1", Application = names[i < 12 ? 0 : i < 18 ? 1 : 2],
                        Title = "Datos de prueba para informes", Activity = 300, Time = 300, Inactivity = 0,
                        FocusTime = focus, Date = focus.ToUniversalTime(), Frecuency = "300", UploadFrecuency = "300"
                    };
                    activity.ReplaceOne(x => x._id == record._id && x.IdEmpresa == company && x.Pc == TestPc, record, new ReplaceOptions { IsUpsert = true });
                }
                for (var i = 0; i < 3; i++)
                {
                    var record = new InstalledProgramsViewModel {
                        _id = SampleId(100 + i), IdCompany = company, User = employee.Usuario, Pc = TestPc,
                        Name = names[i], Vertion = "1.0 (prueba)", Size = "100 MB", InstalledDate = date.Value.Date, Status = true
                    };
                    software.ReplaceOne(x => x._id == record._id && x.IdCompany == company && x.Pc == TestPc, record, new ReplaceOptions { IsUpsert = true });
                }
                var types = new[] { "Procesador", "Memoria", "Disco" };
                var descriptions = new[] { "Procesador de prueba, 4 núcleos", "Memoria de prueba, 16 GB", "Disco de prueba, 512 GB" };
                for (var i = 0; i < 3; i++)
                {
                    var record = new InstalledHardwareViewModel {
                        _id = SampleId(200 + i), IdCompany = company, User = employee.Usuario, Pc = TestPc,
                        Type = types[i], Hardware = descriptions[i], date = date.Value.Date, status = true
                    };
                    hardware.ReplaceOne(x => x._id == record._id && x.IdCompany == company && x.Pc == TestPc, record, new ReplaceOptions { IsUpsert = true });
                }
                ViewBag.Result = "Carga completada: 24 registros de actividad (2 horas, de 08:00 a 10:00), 3 programas y 3 componentes de hardware. Fecha: " + date.Value.ToString("dd/MM/yyyy") + ".";
                if (missing) ViewBag.ClassificationWarning = "Faltan clasificaciones de programas de uno o más tipos. Clasifica los programas PRUEBA en Clasificación de programas para revisar productividad, improductividad y neutralidad. Las clasificaciones existentes no se modificaron.";
                return View("Index", date.Value.Date);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Carga de informes de prueba: {0}", ex);
                ModelState.AddModelError("", "No se pudo completar la carga. Revisa los logs y las conexiones SQL/MongoDB. Puedes reintentar sin duplicar los registros; una carga parcial también se puede eliminar desde esta página.");
                return View("Index", date ?? DateTime.Today);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Clear()
        {
            try
            {
                var employee = GetEmployee();
                MongoHelper.ConnectToMongoService();
                var company = CompanyId.ToString();
                var a = MongoHelper.database.GetCollection<TrakerBase>("TrackerTime").DeleteMany(x => x.IdEmpresa == company && x.Pc == TestPc && x.UserName == employee.Usuario).DeletedCount;
                var s = MongoHelper.database.GetCollection<InstalledProgramsViewModel>("Software").DeleteMany(x => x.IdCompany == company && x.Pc == TestPc && x.User == employee.Usuario).DeletedCount;
                var h = MongoHelper.database.GetCollection<InstalledHardwareViewModel>("Hardware").DeleteMany(x => x.IdCompany == company && x.Pc == TestPc && x.User == employee.Usuario).DeletedCount;
                ViewBag.EmployeeUser = employee.Usuario;
                ViewBag.Result = string.Format("Datos de prueba eliminados: {0} registros de actividad, {1} programas y {2} componentes de hardware.", a, s, h);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Limpieza de informes de prueba: {0}", ex);
                ModelState.AddModelError("", "No se pudo completar la limpieza. Revisa los logs y vuelve a intentarlo.");
            }
            return View("Index", DateTime.Today);
        }

        private static MongoDB.Bson.BsonBinaryData SampleId(int index)
        {
            var bytes = EmployeeId.ToByteArray();
            bytes[12] = 0xDE; bytes[13] = 0xDA;
            bytes[14] = (byte)(index >> 8); bytes[15] = (byte)index;
            return new MongoDB.Bson.BsonBinaryData(new Guid(bytes), MongoDB.Bson.GuidRepresentation.Standard);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
