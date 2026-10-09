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
        private static readonly Guid[] EmployeeIds =
        {
            new Guid("EDA88BA8-1870-4313-9ACF-07127C6DC38E"),
            new Guid("37BB8672-BFBB-4C52-B0DA-239BA029780C"),
            new Guid("71DEDA56-A3F2-49C9-ACEF-B42A310115C5")
        };
        private static readonly Guid CompanyId = new Guid("50DF72C2-565E-4C14-B7FC-1A6A5AC562FD");
        private const string TestPcPrefix = "TEC-PRUEBA-";
        private readonly QueueContext db = new QueueContext();

        private List<Agent_Employee> GetEmployees()
        {
            Guid sessionCompany;
            if (!Request.IsLocal || !Guid.TryParse(Convert.ToString(Session["Company"]), out sessionCompany) || sessionCompany != CompanyId)
                throw new InvalidOperationException("Esta herramienta solo está disponible desde localhost, dentro de la empresa de prueba indicada.");
            var employees = db.Agent_Employee.Where(e => EmployeeIds.Contains(e.idEmployee) && e.IdCompany == CompanyId).ToList();
            if (employees.Count != EmployeeIds.Length || employees.Any(e => string.IsNullOrWhiteSpace(e.Usuario)))
                throw new InvalidOperationException("No se encontraron los tres empleados en esta empresa o alguno no tiene un usuario asociado.");
            return EmployeeIds.Select(id => employees.Single(e => e.idEmployee == id)).ToList();
        }

        [HttpGet]
        public ActionResult Index()
        {
            try { ViewBag.EmployeeUser = string.Join(", ", GetEmployees().Select(e => e.Usuario)); }
            catch (InvalidOperationException ex) { return new HttpStatusCodeResult(403, ex.Message); }
            return View(DateTime.Today);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Load(DateTime? date)
        {
            try
            {
                var employees = GetEmployees();
                ViewBag.EmployeeUser = string.Join(", ", employees.Select(e => e.Usuario));
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
                // Stable identifiers make a retry replace each employee's sample, even after a partial failure.
                for (var employeeIndex = 0; employeeIndex < employees.Count; employeeIndex++)
                {
                    var employee = employees[employeeIndex];
                    var testPc = TestPcFor(employee.idEmployee);
                    for (var i = 0; i < 24; i++)
                    {
                        var focus = DateTime.SpecifyKind(date.Value.Date.AddHours(8).AddMinutes(i * 5), DateTimeKind.Local);
                        var applicationIndex = ApplicationIndexFor(employeeIndex, i);
                        var record = new TrakerBase {
                            _id = SampleId(employee.idEmployee, i), IdEmpresa = company, UserName = employee.Usuario, Pc = testPc,
                            Ip = "127.0.0.1", Application = names[applicationIndex],
                            Title = "Datos de prueba para informes - escenario " + (employeeIndex + 1), Activity = 300, Time = 300, Inactivity = 0,
                            FocusTime = focus, Date = focus.ToUniversalTime(), Frecuency = "300", UploadFrecuency = "300"
                        };
                        activity.ReplaceOne(x => x._id == record._id && x.IdEmpresa == company && x.Pc == testPc, record, new ReplaceOptions { IsUpsert = true });
                    }
                    for (var i = 0; i < 3; i++)
                    {
                        var record = new InstalledProgramsViewModel {
                            _id = SampleId(employee.idEmployee, 100 + i), IdCompany = company, User = employee.Usuario, Pc = testPc,
                            Name = names[i], Vertion = (1 + employeeIndex) + "." + i + " (prueba)", Size = (100 + employeeIndex * 50 + i * 25) + " MB", InstalledDate = date.Value.Date, Status = true
                        };
                        software.ReplaceOne(x => x._id == record._id && x.IdCompany == company && x.Pc == testPc, record, new ReplaceOptions { IsUpsert = true });
                    }
                    var types = new[] { "Procesador", "Memoria", "Disco" };
                    var descriptions = new[] {
                        "Procesador de prueba, " + (4 + employeeIndex * 2) + " núcleos",
                        "Memoria de prueba, " + (16 + employeeIndex * 8) + " GB",
                        "Disco de prueba, " + (512 + employeeIndex * 256) + " GB"
                    };
                    for (var i = 0; i < 3; i++)
                    {
                        var record = new InstalledHardwareViewModel {
                            _id = SampleId(employee.idEmployee, 200 + i), IdCompany = company, User = employee.Usuario, Pc = testPc,
                            Type = types[i], Hardware = descriptions[i], date = date.Value.Date, status = true
                        };
                        hardware.ReplaceOne(x => x._id == record._id && x.IdCompany == company && x.Pc == testPc, record, new ReplaceOptions { IsUpsert = true });
                    }
                }
                ViewBag.Result = "Carga completada para 3 empleados: 72 registros de actividad (2 horas por empleado, de 08:00 a 10:00), 9 programas y 9 componentes de hardware. Los escenarios comparten las mismas aplicaciones y duración, con distribuciones de uso e inventarios distintos. Fecha: " + date.Value.ToString("dd/MM/yyyy") + ".";
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
                var employees = GetEmployees();
                MongoHelper.ConnectToMongoService();
                var company = CompanyId.ToString();
                var users = employees.Select(e => e.Usuario).ToArray();
                var pcs = employees.Select(e => TestPcFor(e.idEmployee)).ToArray();
                var a = MongoHelper.database.GetCollection<TrakerBase>("TrackerTime").DeleteMany(x => x.IdEmpresa == company && pcs.Contains(x.Pc) && users.Contains(x.UserName)).DeletedCount;
                var s = MongoHelper.database.GetCollection<InstalledProgramsViewModel>("Software").DeleteMany(x => x.IdCompany == company && pcs.Contains(x.Pc) && users.Contains(x.User)).DeletedCount;
                var h = MongoHelper.database.GetCollection<InstalledHardwareViewModel>("Hardware").DeleteMany(x => x.IdCompany == company && pcs.Contains(x.Pc) && users.Contains(x.User)).DeletedCount;
                ViewBag.EmployeeUser = string.Join(", ", users);
                ViewBag.Result = string.Format("Datos de prueba eliminados para 3 empleados: {0} registros de actividad, {1} programas y {2} componentes de hardware.", a, s, h);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Limpieza de informes de prueba: {0}", ex);
                ModelState.AddModelError("", "No se pudo completar la limpieza. Revisa los logs y vuelve a intentarlo.");
            }
            return View("Index", DateTime.Today);
        }

        private static int ApplicationIndexFor(int employeeIndex, int recordIndex)
        {
            if (employeeIndex == 1) return recordIndex % 3;
            if (employeeIndex == 2) return recordIndex < 10 ? 0 : recordIndex < 18 ? 1 : 2;
            return recordIndex < 12 ? 0 : recordIndex < 18 ? 1 : 2;
        }

        private static string TestPcFor(Guid employeeId)
        {
            return TestPcPrefix + employeeId.ToString("N").Substring(0, 8).ToUpperInvariant();
        }

        private static MongoDB.Bson.BsonBinaryData SampleId(Guid employeeId, int index)
        {
            var bytes = employeeId.ToByteArray();
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
