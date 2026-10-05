using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Queue.DAL;
using Queue.Models;
using Queue.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Reflection;
using System.Collections;
using System.Data.Entity;
using System.Net;
using Microsoft.AspNet.Identity;
using Queue.DataBase;
using SharpCompress.Compressors.Xz;

namespace Queue.Controllers
{
    [Authorize]
    public class OperationController : BaseController
    {
        private QueueContext db = new QueueContext();
        private IRepositorio _repositorio;

        public OperationController()
        {
            MongoHelper.ConnectToMongoService();
            _repositorio = new Repositorio();
        }

        #region AddMethods
        public bool AddHardware(List<InstalledHardwareViewModel> o)
        {
            try
            {
                MongoHelper.HardWareList = MongoHelper.database.GetCollection<InstalledHardwareViewModel>("Hardware");
                MongoHelper.HardWareList.InsertMany(o);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool AddSoftware(List<InstalledProgramsViewModel> o)
        {
            try
            {
                MongoHelper.SoftWareList = MongoHelper.database.GetCollection<InstalledProgramsViewModel>("Software");
                MongoHelper.SoftWareList.InsertMany(o);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool AddTracker(List<TrakerBase> tb)
        {
            try
            {
                List<TrakerBase> ltbr = new List<TrakerBase>();
                MongoHelper.TrakerBase = MongoHelper.database.GetCollection<TrakerBase>("TrackerTime");

                //sele restan 5 horas porque no se que pasa con el servidor de mkongo que le suma 5 horas, no se como configurar una UTC acorde con mongo para que ponga
                //la hora que es.
                foreach (var j in tb)
                {
                    double upload = double.Parse(j.UploadFrecuency);

                    j.Date = new DateTime(j.Date.Year, j.Date.Month, j.Date.Day, j.Date.Hour, j.Date.Minute, j.Date.Second, j.Date.Kind).AddHours(-5);
                    j.FocusTime = new DateTime(j.FocusTime.Year, j.FocusTime.Month, j.FocusTime.Day, j.FocusTime.Hour, j.FocusTime.Minute, j.FocusTime.Second, j.FocusTime.Kind).AddHours(-5);

                    if (j.Inactivity > (upload + 5))
                        j.Inactivity = upload;

                    //aqui preguntamos si el tiempo de actividad es mayor al doble de la frecuencia de subida se elimina de la lista,
                    //porque puede ser un bug
                    TrakerBase trb = new TrakerBase();
                    if (j.Activity >= (upload * 2))
                    {
                        ltbr.Add(trb);
                    }
                }
                //se eliminan los que son mayores al doble de actividad, por posible bug
                foreach (var l in ltbr)
                {
                    tb.Remove(l);
                }

                MongoHelper.TrakerBase.InsertMany(tb);

                return true;
            }
            catch (Exception ex)
            {
                LogsViewModel lvm = new LogsViewModel();
                lvm.datelog = DateTime.Now;
                lvm.LogRegister = ex.Message + "-" + ex.InnerException.Message;
                lvm.Module = "AddTracker";
                AddLog(lvm);

                return false;
            }
        }

        public bool AddCapture(CaptureBase cb)
        {
            try
            {
                MongoHelper.UserCapture = MongoHelper.database.GetCollection<CaptureBase>("WindowsCapture");
                cb.Date = cb.Date.AddHours(-5);
                cb.Hour = cb.Date.Hour;
                MongoHelper.UserCapture.InsertOne(cb);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool AddCaptures(List<CaptureBase> captureModel)
        {
            try
            {
                MongoHelper.UserCapture = MongoHelper.database.GetCollection<CaptureBase>("WindowsCapture");
                MongoHelper.UserCapture.InsertMany(captureModel);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool AddLog(LogsViewModel lg)
        {
            try
            {
                MongoHelper.Logs = MongoHelper.database.GetCollection<LogsViewModel>("Log");
                MongoHelper.Logs.InsertOne(lg);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //public bool AddFileTranfer(List<FileTransferViewModel> model)
        //{
        //    try
        //    {
        //        MongoHelper.FileTransfer = MongoHelper.database.GetCollection<FileTransferViewModel>("FileTransfer");
        //        MongoHelper.FileTransfer.InsertMany(model);

        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        #endregion

        #region UpdateMethods
        public void UpdateHardware(InstalledHardwareViewModel o)
        {
            try
            {
                var collection = MongoHelper.database.GetCollection<InstalledHardwareViewModel>("Hardware");
                var builder = Builders<InstalledHardwareViewModel>.Filter;
                var filter = builder.Eq("_id", o._id);
                var update = Builders<InstalledHardwareViewModel>.Update.Set("status", false).Set("uninstalldate", DateTime.Now);
                var result = collection.UpdateOne(filter, update);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateSotfware(InstalledProgramsViewModel o)
        {
            try
            {
                var collection = MongoHelper.database.GetCollection<InstalledProgramsViewModel>("Software");
                var builder = Builders<InstalledProgramsViewModel>.Filter;
                var filter = builder.Eq("_id", o._id);
                //var update = Builders<InstalledProgramsViewModel>.Update.Set("Status", false).Set("uninstalldate", DateTime.Now);
                var result = collection.DeleteOne(filter);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateFileTransfer(FileTransferViewModel model)
        {
            try
            {
                var collection = MongoHelper.database.GetCollection<FileTransferViewModel>("FileTransfer");
                var builder = Builders<FileTransferViewModel>.Filter;
                var filter = builder.Eq("_id", model._id);
                var result = collection.DeleteOne(filter);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region GetMethods
        public List<InstalledProgramsViewModel> GetSoftWare(string IdCompany, string Pc)
        {
            try
            {
                MongoHelper.SoftWareList = MongoHelper.database.GetCollection<InstalledProgramsViewModel>("Software");
                var builder = Builders<InstalledProgramsViewModel>.Filter;
                var filter = builder.Eq("IdCompany", IdCompany) & builder.Eq("Pc", Pc) & builder.Eq("Status", true);

                var results = MongoHelper.SoftWareList.Find(filter).ToList();
                return results;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<InstalledHardwareViewModel> GetHardware(string IdCompany, string Pc)
        {
            try
            {
                MongoHelper.HardWareList = MongoHelper.database.GetCollection<InstalledHardwareViewModel>("Hardware");
                var builder = Builders<InstalledHardwareViewModel>.Filter;
                var filter = builder.Eq("IdCompany", IdCompany) & builder.Eq("Pc", Pc) & builder.Eq("status", true);

                var results = MongoHelper.HardWareList.Find(filter).ToList();
                return results;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<CaptureBase> GeImage(string IdCompany)
        {
            try
            {
                MongoHelper.UserCapture = MongoHelper.database.GetCollection<CaptureBase>("WindowsCapture");
                var builder = Builders<CaptureBase>.Filter;
                var filter = builder.Eq("IdCompany", IdCompany);

                var results = MongoHelper.UserCapture.Find(filter).ToList();
                return results;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<AutomaticTakeTimeModel> GetSoftWareClasification(string IdCompany)
        {
            try
            {
                //MongoHelper.SoftWareList = MongoHelper.database.GetCollection<InstalledProgramsViewModel>("Software");
                //var builder = Builders<InstalledProgramsViewModel>.Filter;

                //var filter = builder.Eq("IdCompany", IdCompany) & builder.Eq("Status", true);

                //var results = MongoHelper.SoftWareList.Find(filter).ToList();
                var query = new List<AutomaticTakeTimeModel>();

                query = (from e in MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>()
                         where e.IdEmpresa == IdCompany
                         select new AutomaticTakeTimeModel
                         {
                             Application = e.Application

                         }).Distinct().ToList();

                if (!query.Any())
                {
                    return getProgramDefautl();
                }

                return query;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<AutomaticTakeTimeModel> getProgramDefautl()
        {
            var query = new List<AutomaticTakeTimeModel>();

            query.Add(new AutomaticTakeTimeModel { Application = "WINWORD" });
            query.Add(new AutomaticTakeTimeModel { Application = "WinRAR" });
            query.Add(new AutomaticTakeTimeModel { Application = "WhatsApp" });
            query.Add(new AutomaticTakeTimeModel { Application = "WerFault" });
            query.Add(new AutomaticTakeTimeModel { Application = "Uninstall JDownloader" });
            query.Add(new AutomaticTakeTimeModel { Application = "testGetUsername" });
            query.Add(new AutomaticTakeTimeModel { Application = "Teams" });
            query.Add(new AutomaticTakeTimeModel { Application = "Taskmgr" });
            query.Add(new AutomaticTakeTimeModel { Application = "taskmgr" });
            query.Add(new AutomaticTakeTimeModel { Application = "SymCorpUI" });
            query.Add(new AutomaticTakeTimeModel { Application = "StartMenuExperienceHost" });
            query.Add(new AutomaticTakeTimeModel { Application = "Ssms" });
            query.Add(new AutomaticTakeTimeModel { Application = "Spotify" });
            query.Add(new AutomaticTakeTimeModel { Application = "SnippingTool" });
            query.Add(new AutomaticTakeTimeModel { Application = "Skype" });
            query.Add(new AutomaticTakeTimeModel { Application = "ShellExperienceHost" });
            query.Add(new AutomaticTakeTimeModel { Application = "setup" });
            query.Add(new AutomaticTakeTimeModel { Application = "SearchHost" });
            query.Add(new AutomaticTakeTimeModel { Application = "SearchApp" });
            query.Add(new AutomaticTakeTimeModel { Application = "ScreenClippingHost" });
            query.Add(new AutomaticTakeTimeModel { Application = "runas" });
            query.Add(new AutomaticTakeTimeModel { Application = "RocketDock" });
            query.Add(new AutomaticTakeTimeModel { Application = "regedit" });
            query.Add(new AutomaticTakeTimeModel { Application = "qemu-system-x86_64" });
            query.Add(new AutomaticTakeTimeModel { Application = "PointDesk" });
            query.Add(new AutomaticTakeTimeModel { Application = "PanGPA" });
            query.Add(new AutomaticTakeTimeModel { Application = "OUTLOOK" });
            query.Add(new AutomaticTakeTimeModel { Application = "OpenWith" });
            query.Add(new AutomaticTakeTimeModel { Application = "OneDrive" });
            query.Add(new AutomaticTakeTimeModel { Application = "obs64" });
            query.Add(new AutomaticTakeTimeModel { Application = "notepad++" });
            query.Add(new AutomaticTakeTimeModel { Application = "Notepad" });
            query.Add(new AutomaticTakeTimeModel { Application = "notepad" });
            query.Add(new AutomaticTakeTimeModel { Application = "mstsc" });
            query.Add(new AutomaticTakeTimeModel { Application = "mspaint" });
            query.Add(new AutomaticTakeTimeModel { Application = "msiexec" });
            query.Add(new AutomaticTakeTimeModel { Application = "msedge" });
            query.Add(new AutomaticTakeTimeModel { Application = "MongoDBCompass" });
            query.Add(new AutomaticTakeTimeModel { Application = "MMX4" });
            query.Add(new AutomaticTakeTimeModel { Application = "mmc" });
            query.Add(new AutomaticTakeTimeModel { Application = "LockApp" });
            query.Add(new AutomaticTakeTimeModel { Application = "LiteDB.Studio" });
            query.Add(new AutomaticTakeTimeModel { Application = "Lightshot" });
            query.Add(new AutomaticTakeTimeModel { Application = "iScrRec" });
            query.Add(new AutomaticTakeTimeModel { Application = "Installer" });
            query.Add(new AutomaticTakeTimeModel { Application = "InetMgr" });
            query.Add(new AutomaticTakeTimeModel { Application = "Idle" });
            query.Add(new AutomaticTakeTimeModel { Application = "FortiClient" });
            query.Add(new AutomaticTakeTimeModel { Application = "firefox" });
            query.Add(new AutomaticTakeTimeModel { Application = "FileActivityWatch" });
            query.Add(new AutomaticTakeTimeModel { Application = "explorer" });
            query.Add(new AutomaticTakeTimeModel { Application = "EXCEL" });
            query.Add(new AutomaticTakeTimeModel { Application = "dxdiag" });
            query.Add(new AutomaticTakeTimeModel { Application = "dwm" });
            query.Add(new AutomaticTakeTimeModel { Application = "DisableUSBDriver" });
            query.Add(new AutomaticTakeTimeModel { Application = "devenv" });
            query.Add(new AutomaticTakeTimeModel { Application = "DB Browser for SQLite" });
            query.Add(new AutomaticTakeTimeModel { Application = "CredentialUIBroker" });
            query.Add(new AutomaticTakeTimeModel { Application = "Code" });
            query.Add(new AutomaticTakeTimeModel { Application = "cmd" });
            query.Add(new AutomaticTakeTimeModel { Application = "chrome" });
            query.Add(new AutomaticTakeTimeModel { Application = "ccSvcHst" });
            query.Add(new AutomaticTakeTimeModel { Application = "brave" });
            query.Add(new AutomaticTakeTimeModel { Application = "AT_Monitor" });
            query.Add(new AutomaticTakeTimeModel { Application = "ApplicationFrameHost" });
            query.Add(new AutomaticTakeTimeModel { Application = "AnyDesk" });

            return query;
        }

        public List<FileTransferViewModel> ReportFileTransfer(string idCompany, string userName, DateTime startDate, DateTime endDate)
        {
            try
            {
                MongoHelper.FileTransfer = MongoHelper.database.GetCollection<FileTransferViewModel>("FileTransfer");
                var builder = Builders<FileTransferViewModel>.Filter;
                var filter = builder.Eq("IdEmpresa", idCompany) & builder.Gte("Date", startDate) & builder.Lte("Date", endDate.AddDays(1));

                if (!string.IsNullOrEmpty(userName))
                    filter &= builder.Eq("UserName", userName);

                var results = MongoHelper.FileTransfer.Find(filter).ToList();
                return results.OrderByDescending(r => r.FechaInsercion).Take(1000).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion

        #region Stadisticas

        public BasicStatsModel MoreUsedApp(string idcompany, DateTime fromdate, DateTime todate)
        {
            BasicStatsModel bm = new BasicStatsModel();
            var query = (from e in MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>()
                         where e.IdEmpresa == idcompany
                         && e.Date >= fromdate && e.Date <= todate
                         select new AutomaticTakeTimeModel
                         {
                             Application = e.Application,
                             Time = e.Activity,
                             Date = e.Date
                         }).Distinct().ToList();

            foreach (var grouping in query.OrderByDescending(x => x.Time).GroupBy(g => g.Application).ToList())
            {
                var item = grouping;

                double? time = query.Where(t => t.Application == item.Key).Select(f => f.Time).Sum();
                bm.labels.Add(item.Key);
                double? totalminutes = 0;

                if (time != null && time > 0)
                    totalminutes = (time / 60);

                bm.data.Add(Math.Round(totalminutes.Value, 2));
            }

            return bm;
        }

        public BasicUserModel GetUsers(string idcompany, DateTime fromdate, DateTime todate)
        {
            BasicUserModel bm = new BasicUserModel();
            var query = (from e in MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>()
                         where e.IdEmpresa == idcompany
                         && e.Date >= fromdate && e.Date <= todate
                         select new AutomaticTakeTimeModel
                         {
                             UserName = e.UserName,
                             Application = e.Application,
                             Time = e.Activity,
                             Date = e.Date,
                         }).Distinct().ToList();

            for (var i = 0; i < query.Count; i++)
            {
                bm.User.Add(query[i].UserName);
                bm.Application.Add(query[i].Application);
                bm.Time.Add((double)query[i].Time);
                bm.DateTime.Add((DateTime)query[i].Date);

            }
            return bm;
        }
        public BasicUserModel GetUserByName(string idcompany, DateTime fromdate, DateTime todate, string Name)
        {
            //string fromDate = fromdate.ToString("yyyy-MM-dd");
            //string toDate = todate.ToString("yyyy-MM-dd");
            BasicUserModel bm = new BasicUserModel();
            var query = (from e in MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>()
                         where e.IdEmpresa == idcompany
                         && e.Date >= fromdate && e.Date <= todate
                         && e.UserName == Name
                         select new AutomaticTakeTimeModel
                         {
                             UserName = e.UserName,
                             Application = e.Application,
                             Time = e.Activity,
                             Date = e.Date
                         }).Distinct().ToList();

            for (var i = 0; i < query.Count; i++)
            {
                bm.User.Add(query[i].UserName);
                bm.Application.Add(query[i].Application);
                bm.Time.Add((double)query[i].Time);
            }
            return bm;
        }
        public BasicStatsModel TypeApp(string idcompany)
        {
            DateTime dateFrom = DateTime.Today;//.AddDays(-50);
            DateTime dateTo = dateFrom.AddHours(23);
            BasicStatsModel bm = new BasicStatsModel();
            var query = (from e in MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>()
                         where e.IdEmpresa == idcompany && e.Date >= dateFrom && e.Date <= dateTo
                         select new AutomaticTakeTimeModel
                         {
                             Application = e.Application,
                             Time = e.Activity,
                             Date = e.Date
                         }).Distinct().ToList();

            foreach (var grouping in query.OrderByDescending(x => x.Time).GroupBy(g => g.Application).ToList())
            {
                var item = grouping;

                double? time = query.Where(t => t.Application == item.Key).Select(f => f.Time).Sum();
                bm.labels.Add(item.Key);
                double? totalminutes = 0;

                if (time != null && time > 0)
                    totalminutes = (time / 60);

                bm.data.Add(Math.Round(totalminutes.Value, 2));
            }

            return bm;
        }

        public BasicStatsModel WebUsedApp(string idcompany, DateTime fromdate, DateTime todate)
        {
            BasicStatsModel bm = new BasicStatsModel();
            var query = (from e in MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>()
                         where e.IdEmpresa == idcompany && e.Application == "chrome"
                         && e.Date >= fromdate && e.Date <= todate
                         select new AutomaticTakeTimeModel
                         {
                             Application = e.Application,
                             Time = e.Activity,
                             Date = e.Date,
                             Title = e.Title
                         }).Distinct().ToList();

            foreach (var grouping in query.OrderByDescending(x => x.Time).GroupBy(g => g.Title).ToList())
            {
                var item = grouping;

                double? time = query.Where(t => t.Title == item.Key).Select(f => f.Time).Sum();
                bm.labels.Add(item.Key);
                double? totalminutes = 0;

                if (time != null && time > 0)
                    totalminutes = (time / 60);

                bm.data.Add(Math.Round(totalminutes.Value, 2));
            }

            return bm;
        }

        //public BasicStatsModel ImproductiveUsedApp(string idcompany, DateTime fromdate, DateTime todate)
        //{
        //    BasicStatsModel bm = new BasicStatsModel();
        //    var query = (from e in MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>()
        //                 where e.IdEmpresa == idcompany
        //                 && e.Date >= fromdate && e.Date <= todate
        //                 select new AutomaticTakeTimeModel
        //                 {
        //                     Application = e.Application,
        //                     Time = e.Activity,
        //                     Date = e.Date
        //                 }).Distinct().ToList();

        //    foreach (var grouping in query.OrderByDescending(x => x.Time).GroupBy(g => g.Application).ToList())
        //    {
        //        var item = grouping;

        //        double? time = query.Where(t => t.Application == item.Key).Select(f => f.Time).Sum();
        //        bm.labels.Add(item.Key);
        //        var date = query.Where(t => t.Application == item.Key).Select(f => f.Date).ToList();

        //        double? totalminutes = 0;
        //        for (int i = 0; i < date.Count; i++)
        //        {
        //            bm.DateTime.Add(date[i].ToString("H:mm:ss"));
        //        }
        //        if (time != null && time > 0)
        //            totalminutes = (time / 60);

        //        bm.data.Add(Math.Round(totalminutes.Value, 2));
        //    }

        //    return bm;
        //}

        public List<SelectListItem> GetActivityUser(string idcompany)
        {
            try
            {
                var _queryFiltre = MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>().
                    Where(e => e.IdEmpresa == idcompany);

                return _queryFiltre.Select(x => x.UserName).Distinct().Select(x => new SelectListItem() { Text = x, Value = x }).ToList();

            }
            catch (Exception ex)
            {
                return new List<SelectListItem>();
            }
        }

        public List<LocationHistory> GetUserLocation(Guid idCompany, string user, DateTime startDate, DateTime endDate)
        {
            try
            {
                var queryPrincipal = from t in db.UserLocation
                                     join r in db.Agent_Employee on t.IdEmployee equals r.idEmployee
                                     where t.IdCompany == idCompany
                                     where r.Usuario == user
                                     where t.Fecha >= startDate && t.Fecha <= endDate
                                     select new LocationHistory
                                     {
                                         Name = r.Nombre,
                                         User = r.Usuario,
                                         Latitude = t.Latitude,
                                         Longitude = t.Longitude,
                                         Date = t.Fecha
                                     };


                return queryPrincipal.ToList();
            }
            catch (Exception ex)
            {
                return new List<LocationHistory>();
            }
        }

        public List<ParameterSystem> GetParameterSystemBd(Guid idCompany)
        {
            try
            {
                var queryPrincipal = from t in db.Agent_Configuration
                                     join r in db.Agent_Empresa on t.IdCompany equals r.IdCompany
                                     where t.IdCompany == idCompany
                                     select new ParameterSystem
                                     {
                                         Id_Configuration = t.Id_Configuration,
                                         InactivityPeriod = t.InactivityPeriod,
                                         CaptureFrecuency = t.CaptureFrecuency,
                                         UploadFrecuency = t.UploadFrecuency,
                                         LocationFrecuency = t.LocationFrecuency,
                                         IdCompany = t.IdCompany,
                                         Company = r.Nombre,
                                         DateCreation = t.DateCreation
                                     };


                return queryPrincipal.ToList();
            }
            catch (Exception ex)
            {
                return new List<ParameterSystem>();
            }
        }

        public List<BasicStatsDashboard> GetDataForDashBoard(string idcompany, DateTime from, DateTime to, string user, Guid idgroup)
        {
            try
            {
                Guid idempresa = Guid.Parse(idcompany);
                List<Agent_ProgramClasification> clasifications = db.Agent_ProgramClasification.Where(t => t.Agent_Empresa.IdCompany == idempresa).ToList();

                var _startTest = new DateTime(from.Year, from.Month, from.Day);
                var _endTest = new DateTime(to.Year, to.Month, to.Day);
                _endTest = _endTest.Add(new TimeSpan(23, 59, 59));

                //var _startTest = new DateTime(2022, 02, 25);
                //var _endTest = new DateTime(2022, 02, 25);
                //_endTest = _endTest.Add(new TimeSpan(23, 59, 59));
                List<BasicStatsDashboard> queryPrincipal = new List<BasicStatsDashboard>();

                List<string> users = new List<string>();

                if (idgroup != null && idgroup != Guid.Empty)
                {
                    users = new List<string>();
                    if (!string.IsNullOrEmpty(user) && user != Guid.Empty.ToString() && user != "Todos")
                        users.Add(user.ToLower());
                    else
                        //users = db.Agent_EmployeeGroupsEmployee.Where(f => f.Agent_EmployeesGroups.idemployeesGroup == idgroup).Select(g => g.Agent_Employee.Usuario.ToLower()).ToList();
                        users = db.WorkAreaEmployee.Where(f => f.IdWorkArea == idgroup).Select(g => g.employee.Usuario.ToLower()).ToList();
                }
                if (!string.IsNullOrEmpty(user) && user != Guid.Empty.ToString() && user != "Todos" && idgroup == null && idgroup == Guid.Empty)
                    users.Add(user.ToLower());


                queryPrincipal = MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>().
                    Where(e => e.IdEmpresa == idcompany && (e.FocusTime >= _startTest && e.FocusTime <= _endTest))
                    .Select(e =>
                               new BasicStatsDashboard
                               {
                                   User = e.UserName,
                                   Application = e.Application,
                                   Time = e.Activity,
                                   Date_ = e.Date
                               }).ToList();


                //foltra por usuario o usuarios
                if (users.Count() > 0)
                    queryPrincipal = queryPrincipal.Where(v => users.Contains(v.User.ToLower())).ToList();


                foreach (var j in queryPrincipal)
                {
                    j.Clasification = clasifications.Where(t => t.name == j.Application).Select(s => s.clasification).SingleOrDefault();
                }

                queryPrincipal = queryPrincipal.OrderBy(o => o.Date_).ToList();


                List<BasicStatsDashboard> queryPrincipaltest = new List<BasicStatsDashboard>();

                queryPrincipaltest = queryPrincipal.OrderByDescending(o => o.Time).ToList();

                return queryPrincipal;

            }
            catch (Exception ex)
            {
                return new List<BasicStatsDashboard>();
            }
        }

        public List<BasicStatsDashboard> GetDataForMostUsedApps(string idcompany, DateTime from, DateTime to, string user, string IdWorkArea)
        {
            try
            {
                Guid idempresa = Guid.Parse(idcompany);
                Guid idWorkArea = Guid.Parse(IdWorkArea);
                List<Agent_ProgramClasification> clasifications = db.Agent_ProgramClasification.Where(t => t.Agent_Empresa.IdCompany == idempresa).ToList();

                var _startTest = new DateTime(from.Year, from.Month, from.Day);
                var _endTest = new DateTime(to.Year, to.Month, to.Day);
                _endTest = _endTest.Add(new TimeSpan(23, 59, 59));

                List<BasicStatsDashboard> queryPrincipal = new List<BasicStatsDashboard>();

                List<string> users = new List<string>();

                users = new List<string>();
                if (!string.IsNullOrEmpty(user) && user != Guid.Empty.ToString() && user != "Todos")
                    users.Add(user.ToLower());
                else
                    users = db.WorkAreaEmployee.Where(f => f.IdWorkArea == idWorkArea).Select(g => g.employee.Usuario.ToLower()).ToList();

                queryPrincipal = MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>().
                    Where(e => e.IdEmpresa == idcompany && (e.FocusTime >= _startTest && e.FocusTime <= _endTest))
                    .Select(e =>
                               new BasicStatsDashboard
                               {
                                   User = e.UserName,
                                   Application = e.Application,
                                   Time = e.Activity,
                                   Date_ = e.Date
                               }).OrderBy(o => o.Date_).ToList();

                queryPrincipal = queryPrincipal.Where(e => users.Contains(e.User.ToLower())).ToList();

                //foreach (var j in queryPrincipal)
                //{
                //    j.Clasification = clasifications.Where(t => t.name == j.Application).Select(s => s.clasification).SingleOrDefault();
                //}


                //queryPrincipal = queryPrincipal.OrderBy(o => o.Date_).ToList();

                //List<BasicStatsDashboard> queryPrincipaltest = new List<BasicStatsDashboard>();

                //queryPrincipaltest = queryPrincipal.OrderByDescending(o => o.Time).ToList();

                return queryPrincipal;

            }
            catch (Exception ex)
            {
                return new List<BasicStatsDashboard>();
            }
        }

        private int GetClasification(List<Agent_ProgramClasification> lc, string applicationname)
        {
            int clasification = 0;
            clasification = lc.Where(t => t.name.ToLower() == applicationname).Select(d => d.clasification).SingleOrDefault();
            return clasification;
        }


        public async Task<List<UsersReportGanttModel>> GetactivityData(string idcompany, DateTime fromdate, DateTime todate, int periods, string[] user)
        {
            if (periods == 0) { periods = 5 * 60; }

            var _queryFiltre = MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>()
                .Where(e => e.IdEmpresa == idcompany);

            List<string> filterusers = new List<string>();
            filterusers = user.ToList();

            // Filtrar por multiusuario, excepto si viene la palabra "Todos"
            if (!filterusers.Contains("Todos"))
                _queryFiltre = _queryFiltre.Where(x => filterusers.Contains(x.UserName));

            var _startTest = new DateTime(fromdate.Year, fromdate.Month, fromdate.Day);
            var _endTest = new DateTime(todate.Year, todate.Month, todate.Day);
            _endTest = _endTest.Add(new TimeSpan(23, 59, 59));

            _queryFiltre = _queryFiltre.Where(s => s.FocusTime >= _startTest && s.FocusTime <= _endTest);

            var queryPrincipal = _queryFiltre
                .GroupBy(e => e.UserName)
                .Select(e =>
                           new UsersReportGanttModel
                           {
                               UserName = e.Key,
                               ReportSytems = e.Select(x => new UsersReportGanttModel.ReportTime()
                               {
                                   Application = x.Application,
                                   FocusTime = x.FocusTime,
                                   Activity = x.Activity,
                                   InActivity = x.Inactivity ?? 0
                               }).ToList()
                           }).ToList();

            var guidIdcompany = Guid.Parse(idcompany);
            var WorkAreaEmployeeList = db.Agent_Employee
                    .Include(e => e.WorkAreaEmployees)
                    .Where(t => t.IdCompany == guidIdcompany && filterusers.Contains(t.Usuario))
                    .SelectMany(e => e.WorkAreaEmployees.Select(wa => wa.IdWorkArea))
                    .ToList();

            var listProgramClasification = db.Agent_ProgramClasification
                .Where(t => t.Agent_Empresa.IdCompany.ToString() == idcompany)
                .ToList();

            var clasificationGroups = db.Agent_ClasificationGroups
                .Include(cg => cg.Agent_ProgramClasifications)
                .Include(cg => cg.Agent_EmployeesGroups)
                .Where(cg => WorkAreaEmployeeList.Contains(cg.Agent_EmployeesGroups.idemployeesGroup))
                .ToList();

            ConcurrentQueue<UsersReportGanttModel> queryAuxParallel = new ConcurrentQueue<UsersReportGanttModel>();

            await Task.FromResult(Parallel.ForEach(queryPrincipal, new ParallelOptions { MaxDegreeOfParallelism = 4 }, (Item) =>
            {
                UsersReportGanttModel InsertArray = new UsersReportGanttModel() { UserName = Item.UserName };

                ConcurrentQueue<UsersReportGanttModel.ReportTime> _ReportSytemsParallel = new ConcurrentQueue<UsersReportGanttModel.ReportTime>();

                Parallel.ForEach(Item.ReportSytems, new ParallelOptions { MaxDegreeOfParallelism = 6 }, (Item2) =>
                {
                    string _NameApps = Item2.Application.Trim().ToUpper();
                    int IdClassication = 0;
                    string NameClassication = "Sin clasificar";

                    // Buscar la clasificación del programa
                    var programClasification = listProgramClasification
                        .FirstOrDefault(pc => pc.name.Trim().ToUpper() == _NameApps);

                    if (programClasification != null)
                    {
                        // Usar directamente la clasificación del programa
                        IdClassication = programClasification.clasification;

                        if (IdClassication == 1)
                            NameClassication = "Productivas";
                        else if (IdClassication == 2)
                            NameClassication = "Improductiva";
                        else if (IdClassication == 3)
                            NameClassication = "Neutrales";
                        else
                            NameClassication = "Sin clasificar";
                    }

                    var NewReportTimes = new UsersReportGanttModel.ReportTime
                    {
                        Application = _NameApps,
                        FocusTime = Item2.FocusTime,
                        Activity = Item2.Activity,
                        InActivity = Item2.InActivity,
                        AppsImproClassify = IdClassication,
                        AppImproName = NameClassication
                    };

                    _ReportSytemsParallel.Enqueue(NewReportTimes);
                });

                InsertArray.ReportSytems = _ReportSytemsParallel.OrderBy(x => x.FocusTime).ToList();
                queryAuxParallel.Enqueue(InsertArray);
            }));

            List<UsersReportGanttModel> queryAux = new List<UsersReportGanttModel>();

            foreach (var item in queryAuxParallel)
            {
                var NewRecord = new UsersReportGanttModel() { UserName = item.UserName };
                var listAppsGroupClasification = item.ReportSytems.Select(x => x.AppsImproClassify).Distinct().ToList();

                List<UsersReportGanttModel.ReportTime> _reportTimestmp = new List<UsersReportGanttModel.ReportTime>();

                var RecordTimes = item.ReportSytems.OrderBy(c => c.FocusTime);

                string _IdAppsPrevious = string.Empty;

                foreach (var itemReportTimes in RecordTimes)
                {
                    var insertData = true;
                    if (_IdAppsPrevious == itemReportTimes.Application)
                    {
                        var infoTimesApps = _reportTimestmp.Where(x => x.Application == itemReportTimes.Application).OrderBy(x => x.FocusTimeEnd);
                        if (infoTimesApps.Any())
                        {
                            var timesApps = infoTimesApps.Last();

                            TimeSpan diff = timesApps.FocusTimeEnd - itemReportTimes.FocusTime;
                            double _Seconds = Math.Abs(Math.Truncate(diff.TotalSeconds));
                            if (!(_Seconds > 0))
                            {
                                timesApps.Activity += itemReportTimes.Activity;
                                timesApps.InActivity += itemReportTimes.InActivity;
                                insertData = false;
                            }
                        }
                    }

                    if (insertData)
                    {
                        var NewReportTimes = new UsersReportGanttModel.ReportTime
                        {
                            Application = itemReportTimes.Application,
                            FocusTime = itemReportTimes.FocusTime,
                            Activity = itemReportTimes.Activity,
                            InActivity = itemReportTimes.InActivity,
                            AppsImproClassify = itemReportTimes.AppsImproClassify,
                            AppImproName = itemReportTimes.AppImproName
                        };

                        _reportTimestmp.Add(NewReportTimes);
                    }

                    _IdAppsPrevious = itemReportTimes.Application;
                }

                double _periods = periods * 60;
                _reportTimestmp = _reportTimestmp.OrderBy(x => x.FocusTime).ToList();
                var firtTime = true;
                var insertTimesfirst = false;

                foreach (var itemReportTimes in _reportTimestmp)
                {
                    bool newGroup = false;

                    if (firtTime)
                    {
                        firtTime = false;
                        var NewReportTimes = new UsersReportGanttModel.ReportTime
                        {
                            Application = itemReportTimes.Application,
                            FocusTime = itemReportTimes.FocusTime,
                            Activity = 0,
                            InActivity = 0,
                            AppsImproClassify = itemReportTimes.AppsImproClassify,
                            AppImproName = itemReportTimes.AppImproName
                        };

                        insertTimesfirst = true;

                        NewRecord.ReportSytems.Add(NewReportTimes);
                    }
                    else
                    {
                        insertTimesfirst = false;
                    }

                    var foundRecordTimes = NewRecord.ReportSytems.Last();

                    if (_periods > 0)
                    {
                        if ((foundRecordTimes.TotalActivity + itemReportTimes.TotalActivity) > _periods)
                        {
                            newGroup = true;
                        }
                    }

                    if (newGroup)
                    {
                        double _activitys = itemReportTimes.Activity;
                        double _inactivitys = itemReportTimes.InActivity;
                        DateTime _FocusTime = itemReportTimes.FocusTime;

                        if (insertTimesfirst)
                        {
                            insertTimesfirst = false;
                            foundRecordTimes.Activity = _activitys;
                            foundRecordTimes.InActivity = _inactivitys;
                        }

                        if (foundRecordTimes.TotalActivity > _periods)
                        {
                            if (foundRecordTimes.Activity < _periods)
                            {
                                double _dife = _periods - foundRecordTimes.Activity;
                                double _restante = (foundRecordTimes.InActivity > 0 ? foundRecordTimes.InActivity - _dife : 0);

                                _activitys = 0;
                                _inactivitys = _restante;

                                foundRecordTimes.InActivity = (foundRecordTimes.InActivity > 0 ? _dife : 0);
                            }
                            else
                            {
                                double _dif = foundRecordTimes.Activity - _periods;

                                _activitys = _dif;
                                _inactivitys = foundRecordTimes.InActivity;

                                foundRecordTimes.Activity = _periods;
                                foundRecordTimes.InActivity = 0;
                            }

                            foundRecordTimes.GroupApplication.Add(new UsersReportGanttModel.ReportTimeGroupApps()
                            {
                                Application = foundRecordTimes.Application,
                                AppImproName = foundRecordTimes.AppImproName,
                                AppsImproClassify = foundRecordTimes.AppsImproClassify,
                                Activity = foundRecordTimes.Activity,
                                InActivity = foundRecordTimes.InActivity
                            });
                        }

                        _FocusTime = foundRecordTimes.FocusTimeEnd;

                        double _aux = 0;
                        bool contineCorrection = true;
                        while (contineCorrection)
                        {
                            var _sumActivitys = _aux + (_activitys + _inactivitys);
                            if (_sumActivitys > _periods)
                            {
                                double auxActivity = 0;
                                double auxInActivity = 0;

                                if (_activitys < _periods)
                                {
                                    double _dife = _periods - _activitys;
                                    double _restante = (_inactivitys > 0 ? _inactivitys - _dife : 0);

                                    auxActivity = _activitys;
                                    auxInActivity = (_inactivitys > 0 ? _dife : 0);

                                    _activitys = 0;
                                    _inactivitys = _restante;
                                }
                                else
                                {
                                    double _dif = _activitys - _periods;

                                    _activitys = _dif;

                                    auxActivity = _periods;
                                    auxInActivity = 0;
                                }

                                var timeSpan = new TimeSpan(_FocusTime.TimeOfDay.Hours, _FocusTime.TimeOfDay.Minutes, _FocusTime.TimeOfDay.Seconds);
                                var infoFound = NewRecord.ReportSytems.Where(x => (new TimeSpan(x.FocusTime.TimeOfDay.Hours, x.FocusTime.TimeOfDay.Minutes, x.FocusTime.TimeOfDay.Seconds)).CompareTo(timeSpan) == 0).ToList();
                                if (infoFound.Any())
                                {
                                    //_FocusTime = infoFound.First().FocusTimeEnd;
                                }

                                var _NewReportTimes = new UsersReportGanttModel.ReportTime
                                {
                                    Application = foundRecordTimes.Application,
                                    GroupApplication = new List<UsersReportGanttModel.ReportTimeGroupApps>() {
                                new UsersReportGanttModel.ReportTimeGroupApps()
                                {
                                    Application = itemReportTimes.Application,
                                    AppImproName = itemReportTimes.AppImproName,
                                    AppsImproClassify = itemReportTimes.AppsImproClassify,
                                    Activity = auxActivity,
                                    InActivity = auxInActivity
                                }
                            },
                                    FocusTime = _FocusTime,
                                    Activity = auxActivity,
                                    InActivity = auxInActivity,
                                    AppsImproClassify = itemReportTimes.AppsImproClassify,
                                    AppImproName = itemReportTimes.AppImproName
                                };

                                NewRecord.ReportSytems.Add(_NewReportTimes);

                                _FocusTime = _NewReportTimes.FocusTimeEnd;
                            }
                            else
                            {
                                contineCorrection = false;
                                _activitys = Math.Abs(_activitys);
                                _inactivitys = Math.Abs(_inactivitys);
                            }
                        }

                        var timeSpan2 = new TimeSpan(_FocusTime.TimeOfDay.Hours, _FocusTime.TimeOfDay.Minutes, _FocusTime.TimeOfDay.Seconds);
                        var infoFounds = NewRecord.ReportSytems.Where(x => (new TimeSpan(x.FocusTime.TimeOfDay.Hours, x.FocusTime.TimeOfDay.Minutes, x.FocusTime.TimeOfDay.Seconds)).CompareTo(timeSpan2) == 0).ToList();
                        if (infoFounds.Any())
                        {
                            //_FocusTime = infoFounds.First().FocusTimeEnd;
                        }

                        var NewReportTimes = new UsersReportGanttModel.ReportTime
                        {
                            Application = itemReportTimes.Application,
                            GroupApplication = new List<UsersReportGanttModel.ReportTimeGroupApps>() {
                        new UsersReportGanttModel.ReportTimeGroupApps()
                        {
                            Application = itemReportTimes.Application,
                            AppImproName = itemReportTimes.AppImproName,
                            AppsImproClassify = itemReportTimes.AppsImproClassify,
                            Activity = _activitys,
                            InActivity = _inactivitys
                        }
                    },
                            FocusTime = _FocusTime,
                            Activity = _activitys,
                            InActivity = _inactivitys,
                            AppsImproClassify = itemReportTimes.AppsImproClassify,
                            AppImproName = itemReportTimes.AppImproName
                        };

                        NewRecord.ReportSytems.Add(NewReportTimes);
                    }
                    else
                    {
                        
                        foundRecordTimes.GroupApplication.Add(new UsersReportGanttModel.ReportTimeGroupApps() { Application = itemReportTimes.Application, AppImproName = itemReportTimes.AppImproName, AppsImproClassify = itemReportTimes.AppsImproClassify, Activity = itemReportTimes.Activity, InActivity = itemReportTimes.InActivity });
                        foundRecordTimes.Activity += itemReportTimes.Activity;
                        foundRecordTimes.InActivity += itemReportTimes.InActivity;
                    }
                }

                if (NewRecord.ReportSytems.Any())
                {
                    queryAux.Add(NewRecord);
                }
            }

            return queryAux;
        }



        public async Task<List<UsersReportGanttModel>> GetactivityDataUserSelected(string idcompany, DateTime fromdate, DateTime todate, string user)
        {

            var _queryFiltre = MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>().
                Where(e => e.IdEmpresa == idcompany);

            _queryFiltre = _queryFiltre.Where(x => x.UserName == user);

            var _startTest = new DateTime(fromdate.Year, fromdate.Month, fromdate.Day);
            var _endTest = new DateTime(todate.Year, todate.Month, todate.Day);
            _endTest = _endTest.Add(new TimeSpan(23, 59, 59));

            _queryFiltre = _queryFiltre.Where(s => s.FocusTime >= _startTest && s.FocusTime <= _endTest);

            var queryPrincipal = _queryFiltre.
                GroupBy(e => e.UserName)
                .Select(e =>
                           new UsersReportGanttModel
                           {
                               UserName = e.Key,
                               ReportSytems = e.Select(x => new UsersReportGanttModel.ReportTime()
                               {
                                   Application = x.Application,
                                   FocusTime = x.FocusTime,
                                   Activity = x.Activity,
                                   InActivity = x.Inactivity ?? 0,
                                   URL = x.Url
                               }).ToList()
                           }).ToList();



            ConcurrentQueue<UsersReportGanttModel> queryAuxParallel = new ConcurrentQueue<UsersReportGanttModel>();
            var guidIdcompany = Guid.Parse(idcompany);

            var WorkAreaEmployeeList = db.Agent_Employee
                    .Include(e => e.WorkAreaEmployees)
                    .Where(t => t.IdCompany == guidIdcompany && t.Usuario == user)
                    .SelectMany(e => e.WorkAreaEmployees.Select(wa => wa.IdWorkArea))
                    .ToList();

            //var clasificationGroups = db.Agent_ClasificationGroups
            //        .Include(cg => cg.Agent_ProgramClasifications)
            //        .Include(cg => cg.Agent_EmployeesGroups)
            //        .Where(cg => WorkAreaEmployeeList.Contains(cg.Agent_EmployeesGroups.idemployeesGroup))
            //        .Select(cg => new
            //        {
            //            ProgramName = cg.Agent_ProgramClasifications.name,
            //            GroupName = cg.Agent_EmployeesGroups.Nombre,
            //            Clasification = cg.clasification
            //        })
            //        .ToList();

            var listProgramClasification = db.Agent_ProgramClasification.Where(t => t.Agent_Empresa.IdCompany.ToString() == idcompany).ToList();

            await Task.FromResult(Parallel.ForEach(queryPrincipal, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (Item) =>
            {
                UsersReportGanttModel InsertArray = new UsersReportGanttModel() { UserName = Item.UserName };

                ConcurrentQueue<UsersReportGanttModel.ReportTime> _ReportSytemsParallel = new ConcurrentQueue<UsersReportGanttModel.ReportTime>();

                await Task.FromResult(Parallel.ForEach(Item.ReportSytems, new ParallelOptions { MaxDegreeOfParallelism = 6 }, (Item2) =>
                {
                    string _NameApps = Item2.Application.Trim().ToUpper();
                    int IdClassication = 0;
                    string NameClassication = "Sin clasificar";

                    using (DAL.QueueContext db = new DAL.QueueContext())
                    {


                        var programsFound = db.Agent_ClasificationGroups
                        .Include(cg => cg.Agent_ProgramClasifications)
                        .Include(cg => cg.Agent_EmployeesGroups)
                        .Where(cg => WorkAreaEmployeeList.Contains(cg.Agent_EmployeesGroups.idemployeesGroup) && cg.Agent_ProgramClasifications.name.ToUpper() == _NameApps)
                        .Select(cg => new
                        {
                            cg.clasification
                        })
                        .FirstOrDefault();

                        if (programsFound != null)
                        {
                            switch (programsFound.clasification)
                            {
                                case 1:
                                    IdClassication = 1;
                                    NameClassication = "Productivas";
                                    break;
                                case 2:
                                    IdClassication = 2;
                                    NameClassication = "Improductiva";
                                    break;
                                case 3:
                                    IdClassication = 3;
                                    NameClassication = "Neutrales";
                                    break;
                            }
                        }
                    }

                    var NewReportTimes = new UsersReportGanttModel.ReportTime
                    {
                        Application = _NameApps,
                        FocusTime = Item2.FocusTime,
                        Activity = Item2.Activity,
                        InActivity = Item2.InActivity,
                        AppsImproClassify = IdClassication,
                        AppImproName = NameClassication,
                        URL = Item2.URL
                    };

                    _ReportSytemsParallel.Enqueue(NewReportTimes);


                }));

                InsertArray.ReportSytems = _ReportSytemsParallel.OrderBy(x => x.FocusTime).ToList();

                queryAuxParallel.Enqueue(InsertArray);

            }));


            List<UsersReportGanttModel> queryAux = new List<UsersReportGanttModel>();

            foreach (var item in queryAuxParallel)
            {


                List<UsersReportGanttModel.ReportTime> _reportTimestmp = new List<UsersReportGanttModel.ReportTime>();

                var RecordTimes = item.ReportSytems.OrderBy(c => c.FocusTime);
                string _IdAppsPrevious = string.Empty;

                foreach (var itemReportTimes in RecordTimes)
                {
                    var insertData = true;
                    if (_IdAppsPrevious == itemReportTimes.Application)
                    {
                        var sumaActivityRecordAppsLocal = itemReportTimes.Activity;
                        var infoTimesApps = _reportTimestmp.Where(x => x.Application == itemReportTimes.Application).OrderBy(x => x.FocusTimeEnd);
                        if (infoTimesApps.Any())
                        {
                            var timesApps = infoTimesApps.Last();

                            TimeSpan diff = timesApps.FocusTimeEnd - itemReportTimes.FocusTime;
                            double _Seconds = Math.Abs(Math.Truncate(diff.TotalSeconds));
                            if (!(_Seconds > 0))
                            {
                                timesApps.Activity += itemReportTimes.Activity;
                                timesApps.InActivity += itemReportTimes.InActivity;
                                insertData = false;
                            }
                        }
                    }

                    if (insertData)
                    {
                        var NewReportTimes = new UsersReportGanttModel.ReportTime
                        {
                            Application = itemReportTimes.Application,
                            FocusTime = itemReportTimes.FocusTime,
                            Activity = itemReportTimes.Activity,
                            InActivity = itemReportTimes.InActivity,
                            AppsImproClassify = itemReportTimes.AppsImproClassify,
                            AppImproName = itemReportTimes.AppImproName,
                            URL = itemReportTimes.URL
                        };

                        _reportTimestmp.Add(NewReportTimes);
                    }

                    _IdAppsPrevious = itemReportTimes.Application;
                }

                var NewRecord = new UsersReportGanttModel() { UserName = item.UserName };
                _reportTimestmp = _reportTimestmp.OrderBy(x => x.FocusTime).ToList();
                var firtTime = true;


                var _FocusTime = _reportTimestmp.FirstOrDefault().FocusTime;

                foreach (var itemReportTimes in _reportTimestmp)
                {

                    var NewReportTimes = new UsersReportGanttModel.ReportTime
                    {
                        Application = itemReportTimes.Application,
                        GroupApplication = new List<UsersReportGanttModel.ReportTimeGroupApps>() { new UsersReportGanttModel.ReportTimeGroupApps() { Application = itemReportTimes.Application, AppImproName = itemReportTimes.AppImproName, AppsImproClassify = itemReportTimes.AppsImproClassify, Activity = itemReportTimes.Activity, InActivity = itemReportTimes.InActivity } },
                        FocusTime = _FocusTime,
                        Activity = itemReportTimes.Activity,
                        InActivity = itemReportTimes.InActivity,
                        AppsImproClassify = itemReportTimes.AppsImproClassify,
                        AppImproName = itemReportTimes.AppImproName,
                        URL = itemReportTimes.URL
                    };

                    NewRecord.ReportSytems.Add(NewReportTimes);

                    _FocusTime = NewReportTimes.FocusTimeEnd;
                }

                var list_apps = NewRecord.ReportSytems.Select(x => x.Application).Distinct();
                foreach (var itemNameApps in list_apps)
                {
                    var _NewRecord = new UsersReportGanttModel() { UserName = itemNameApps };

                    foreach (var itemTimeTrack in NewRecord.ReportSytems.Where(x => x.Application == itemNameApps).OrderBy(x => x.FocusTime))
                    {
                        _NewRecord.ReportSytems.Add(itemTimeTrack);
                    }

                    queryAux.Add(_NewRecord);
                }
            }

            return queryAux;
        }

        public BasicStatsModel ImproductiveUsedApp(string idcompany, DateTime fromdate, DateTime todate)
        {
            BasicStatsModel bm = new BasicStatsModel();
            var query = (from e in MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>()
                         where e.IdEmpresa == idcompany
                         && e.Date >= fromdate && e.Date <= todate
                         select new AutomaticTakeTimeModel
                         {
                             Application = e.Application,
                             Time = e.Activity,
                             Date = e.Date,
                         }).Distinct().ToList();

            foreach (var grouping in query.OrderByDescending(x => x.Time).GroupBy(g => g.Application).ToList())
            {
                var item = grouping;

                double? time = query.Where(t => t.Application == item.Key).Select(f => f.Time).Sum();
                bm.labels.Add(item.Key);
                var date = query.Where(t => t.Application == item.Key).Select(f => f.Date).ToList();

                double? totalminutes = 0;
                for (int i = 0; i < date.Count; i++)
                {
                    bm.DateTime.Add(date[i].ToString("H:mm:ss"));
                }
                if (time != null && time > 0)
                    totalminutes = (time / 60);

                bm.data.Add(Math.Round(totalminutes.Value, 2));
            }

            return bm;
        }

        public BasicStatsDate DateUsedApp(string app, DateTime fromdate, DateTime todate)
        {
            BasicStatsDate bm = new BasicStatsDate();
            var query = (from e in MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>()
                         where e.Application == app
                         && e.Date >= fromdate && e.Date <= todate
                         select new AutomaticTakeTimeModel
                         {
                             Application = e.Application,
                             Time = e.Activity,
                             Date = e.Date
                         }).Distinct().ToList();

            foreach (var grouping in query.OrderByDescending(x => x.Time).GroupBy(g => g.Application).ToList())
            {
                var item = grouping;

                double? time = query.Where(t => t.Application == item.Key).Select(f => f.Time).Sum();

                var date = query.Where(t => t.Application == item.Key).Select(f => f.Date).ToList();


                for (int i = 0; i < date.Count; i++)
                {
                    bm.DateTime.Add(date[i].ToString("H:mm:ss"));
                }

            }

            return bm;
        }

        public async Task<JsonResult> GetFileTransfer(DateTime startDate, DateTime endDate, string user)
        {
            try
            {
                var _resService = ReportFileTransfer(Session["Company"].ToString(), user, startDate, endDate);

                return Json(_resService.Select(ft => new
                {
                    ft.ProcessServices,
                    ft.MachineName,
                    ft.ProcessPath,
                    ft.FileName,
                    ProcessId = ft.ProcessID,
                    ft.ProcessName,
                    Username = ft.UserName,
                    ft.Path,
                    ft.Date,
                    ft.DataTransmission,
                    ft.ProcessID,
                    ft.LastWriteTime,
                    ft.FechaInsercion,
                    ft.DayOfWeek,
                    ft.TypeOfEvent,
                    ft.TimeOfDay,
                    ft.IsFile,
                    ft.IsDirectory,
                    ft.DiskType,
                    ft.Disk
                }).ToList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> GetHistory(string user, DateTime startDate, DateTime endDate)
        {
            try
            {
                Guid IdCompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
                var _resService = GetUserLocation(IdCompany, user, startDate, endDate);

                return Json(_resService.Select(ft => new
                {
                    Name = ft.Name,
                    User = ft.User,
                    Longitude = ft.Longitude,
                    Latitude = ft.Latitude,
                    Date = ft.Date.ToString("MM/dd/yyyy h:mm tt")//ft.Date
                }).ToList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> GetParameterSystem()
        {
            try
            {
                var IdCompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
                var _resService = GetParameterSystemBd(IdCompany);

                return Json(_resService.Select(ft => new
                {
                    ft.Id_Configuration,
                    ft.Company,
                    ft.InactivityPeriod,
                    ft.CaptureFrecuency,
                    ft.UploadFrecuency,
                    ft.LocationFrecuency,
                    DateCreation = ft.DateCreation.ToString("MM/dd/yyyy")
                }).ToList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult EditParameterSystem(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var IdCompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
            var paramatros = GetParameterSystemBd(IdCompany);
            var paramatro = paramatros.FirstOrDefault(p => p.Id_Configuration == id);
            if (paramatro == null)
            {
                return HttpNotFound();
            }

            return View(paramatro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditParameterSystem(ParameterSystem paramater)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var configuracion = db.Agent_Configuration.Where(t => t.Id_Configuration == paramater.Id_Configuration).FirstOrDefault();
                    configuracion.InactivityPeriod = paramater.InactivityPeriod;
                    configuracion.CaptureFrecuency = paramater.CaptureFrecuency;
                    configuracion.UploadFrecuency = paramater.UploadFrecuency;
                    configuracion.LocationFrecuency = paramater.LocationFrecuency;

                    db.SaveChanges();
                }

                return View("ParameterSystem");
            }
            catch (Exception ex)
            {
                Warning("Error en la actualización de datos.", string.Empty);
                return View();
            }

        }

        #endregion

        #region Reports
        public ActionResult SoftwareReport(string user, Guid? idgroup)
        {
            List<SoftwareReport> srlist = new List<SoftwareReport>();
            Guid IdCompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

            if (IdCompany != Guid.Empty)
            {
                // Validamos si se seleccionó grupo o usuario
                if (!string.IsNullOrEmpty(user) || idgroup != null)
                {
                    MongoHelper.SoftWareList = MongoHelper.database.GetCollection<InstalledProgramsViewModel>("Software");
                    var builder = Builders<InstalledProgramsViewModel>.Filter;

                    // Filtro base: por empresa y activos
                    var filter = builder.Eq("IdCompany", IdCompany) & builder.Eq("Status", true);
                    List<InstalledProgramsViewModel> results = MongoHelper.SoftWareList.Find(filter).ToList();

                    // ✅ Si se seleccionó un grupo
                    if (idgroup != null && idgroup != Guid.Empty)
                    {
                        List<Agent_Employee> users_ = _repositorio.ListUsuarioArea(IdCompany, idgroup.Value);

                        // Normalizamos todos los usuarios a minúsculas
                        List<string> _users = users_.Select(s => s.Usuario.ToLower()).ToList();

                        results = results.Where(u => _users.Contains(u.User.ToLower())).ToList();
                    }

                    // ✅ Si se seleccionó un usuario específico
                    if (!string.IsNullOrEmpty(user) && user != Guid.Empty.ToString())
                    {
                        var usuarioNormalizado = user.ToLower();
                        results = results.Where(u => u.User.ToLower() == usuarioNormalizado).ToList();
                    }

                    // Agrupar programas
                    foreach (var i in results.GroupBy(g => g.Name))
                    {
                        srlist.Add(new SoftwareReport
                        {
                            program = i.Key,
                            quantity = i.Count()
                        });
                    }
                }
            }

            // Lista de grupos
            List<SelectListItem> sli = CreateList(
                db.Agent_EmployeesGroups.Where(c => c.Agent_Empresa.IdCompany == IdCompany).ToList(),
                "idemployeesGroup", "Nombre", idgroup
            );
            sli.Insert(0, new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() });
            ViewBag.idgroup = sli;

            // Lista de usuarios según grupo
            if (idgroup != null && idgroup != Guid.Empty)
            {
                EmployeeSelectList(idgroup.Value);
            }

            return View(srlist);
        }


        /// <summary>
        /// Function to populate a list in the view
        /// </summary>
        /// <param name="idGroupEmployee">
        /// The id of the group employee
        /// </param>
        /// <returns></returns>
        public SelectList EmployeeSelectList(Guid idGroupEmployee)
        {
            Guid company = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
            var QueryUserbyArea = _repositorio.ListUsuarioArea(company, idGroupEmployee);
            var usersList = new SelectList(QueryUserbyArea, "Usuario", "Nombre");
            ViewData["UserList"] = usersList;
            return usersList;

        }

        /// <summary>
        /// Function to retrieve a list of employees
        /// </summary>
        /// <param name="idGroupEmployee">
        /// the id of the group employee
        /// </param>
        /// <returns>
        /// returns the serialize list of employees
        /// </returns>
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

        public ActionResult SoftwareReportDetails(string name, Guid? idgroup, string user)

        {
            
            List<SoftwareReport> srlist = new List<SoftwareReport>();
            Guid IdCompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

            if (IdCompany != Guid.Empty )
            {
                

                MongoHelper.SoftWareList = MongoHelper.database.GetCollection<InstalledProgramsViewModel>("Software");
                var builder = Builders<InstalledProgramsViewModel>.Filter;
                var filter = builder.Eq("IdCompany", IdCompany) &
                             builder.Eq("Status", true) &
                             builder.Eq("Name", name);

                List<InstalledProgramsViewModel> results = MongoHelper.SoftWareList.Find(filter).ToList();

                if (idgroup != null && idgroup != Guid.Empty)
                {
                    List<Agent_Employee> users_ = _repositorio.ListUsuarioArea(IdCompany, idgroup.Value);
                    List<string> _users = users_.Select(s => s.Usuario.Trim().ToLower()).ToList();

                    results = results
                        .Where(u => u.User != null && _users.Contains(u.User.Trim().ToLower()))
                        .ToList();
                }

                if (!string.IsNullOrEmpty(user))
                {
                    var usuarioNormalizado = user.Trim().ToLower();
                    results = results
                        .Where(u => u.User != null && u.User.Trim().ToLower() == usuarioNormalizado)
                        .ToList();
                }

                    foreach (var i in results.Where(g => g.Name == name))
                {
                    srlist.Add(new SoftwareReport
                    {
                        program = i.Name,
                        agrupation = i.Pc
                    });
                }
            }

            List<SelectListItem> sli = CreateList(
                db.Agent_EmployeesGroups.Where(c => c.Agent_Empresa.IdCompany == IdCompany).ToList(),
                "idemployeesGroup", "Nombre", idgroup
            );
            sli.Insert(0, new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() });
            ViewBag.idgroup = sli;

            List<SelectListItem> sle = CreateList(
                db.Agent_Employee.Where(c => c.IdCompany == IdCompany).ToList(),
                "Usuario", "Usuario", user
            );
            sle.Insert(0, new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() });
            ViewBag.user = sle;

            return View(srlist.Distinct());
        }






        public ActionResult HardwareReport(string user, Guid? idgroup)
        {
            List<HardwareReport> srlist = new List<HardwareReport>();

            Guid IdCompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

            if (IdCompany != Guid.Empty)
            {
                // Obtener la colección de hardware
                MongoHelper.HardWareList = MongoHelper.database.GetCollection<InstalledHardwareViewModel>("Hardware");

                var builder = Builders<InstalledHardwareViewModel>.Filter;
                var idCompanyStr = IdCompany.ToString();

                // Filtro base (por empresa y activos)
                var filter = builder.Eq(x => x.IdCompany, idCompanyStr) & builder.Eq(x => x.status, true);
                List<InstalledHardwareViewModel> results = MongoHelper.HardWareList.Find(filter).ToList();

                // Si se seleccionó un grupo
                if (idgroup != null && idgroup != Guid.Empty)
                {
                    List<Agent_Employee> users_ = _repositorio.ListUsuarioArea(IdCompany, idgroup.Value);

                    // ✅ Normalizamos usuarios a minúsculas
                    List<string> _users = users_
                        .Select(s => s.Usuario.ToLower())
                        .ToList();

                    results = results
                        .Where(u => _users.Contains(u.User.ToLower())) // comparación case-insensitive
                        .ToList();
                }

                // Si se seleccionó un usuario específico
                if (!string.IsNullOrEmpty(user) && user != Guid.Empty.ToString())
                {
                    string usuarioNormalizado = user.ToLower(); // ✅ Normalizamos también aquí

                    results = results
                        .Where(u => u.User.ToLower() == usuarioNormalizado) // comparación case-insensitive
                        .ToList();
                }


                // Agrupar resultados
                foreach (var i in results.GroupBy(g => new { g.Type, g.Hardware }))
                {
                    srlist.Add(new HardwareReport
                    {
                        type = i.Key.Type,
                        hardware = i.Key.Hardware,
                        quantity = i.Count()
                    });
                }
            }

            // Lista de grupos
            List<SelectListItem> sli = CreateList(
                db.Agent_EmployeesGroups.Where(c => c.Agent_Empresa.IdCompany == IdCompany).ToList(),
                "idemployeesGroup", "Nombre", idgroup
            );
            sli.Insert(0, new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() });
            ViewBag.idgroup = sli;

            // Lista de usuarios por grupo
            if (idgroup != null && idgroup != Guid.Empty)
            {
                EmployeeSelectList(idgroup.Value);
            }

            return View(srlist);
        }



        public ActionResult HardwareReportDetails(string hardware, Guid? idgroup, string user)
        {
            List<HardwareReport> srlist = new List<HardwareReport>();
            Guid IdCompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

            if (IdCompany != Guid.Empty && !string.IsNullOrEmpty(hardware))
            {
                MongoHelper.HardWareList = MongoHelper.database.GetCollection<InstalledHardwareViewModel>("Hardware");
                var builder = Builders<InstalledHardwareViewModel>.Filter;
                var filter = builder.Eq("IdCompany", IdCompany) &
                             builder.Eq("status", true) &
                             builder.Eq("Hardware", hardware);

                List<InstalledHardwareViewModel> results = MongoHelper.HardWareList.Find(filter).ToList();

                // 🔹 Filtrar por grupo si aplica
                if (idgroup != null && idgroup != Guid.Empty)
                {
                    List<Agent_Employee> users_ = _repositorio.ListUsuarioArea(IdCompany, idgroup.Value);
                    List<string> _users = users_.Select(s => s.Usuario.Trim().ToLower()).ToList();

                    results = results
                        .Where(u => u.User != null && _users.Contains(u.User.Trim().ToLower()))
                        .ToList();
                }

                // 🔹 Filtrar por usuario si viene seleccionado
                if (!string.IsNullOrEmpty(user) && user != Guid.Empty.ToString())
                {
                    var usuarioNormalizado = user.Trim().ToLower();
                    results = results
                        .Where(u => u.User != null && u.User.Trim().ToLower() == usuarioNormalizado)
                        .ToList();
                }
                // 🔹 Si user es vacío → no filtramos, se muestran todos los del grupo (o todos en general si no hay grupo)

                // Mapear resultados
                foreach (var i in results.Where(g => g.Hardware == hardware))
                {
                    srlist.Add(new HardwareReport
                    {
                        agrupation = i.Pc,
                        type = i.Type,
                        hardware = i.Hardware
                    });
                }
            }

            // Dropdown de grupos
            List<SelectListItem> sli = CreateList(
                db.Agent_EmployeesGroups.Where(c => c.Agent_Empresa.IdCompany == IdCompany).ToList(),
                "idemployeesGroup", "Nombre", idgroup);
            sli.Insert(0, new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() });
            ViewBag.idgroup = sli;

            // Dropdown de usuarios
            List<SelectListItem> sle = CreateList(
                db.Agent_Employee.Where(c => c.IdCompany == IdCompany).ToList(),
                "Usuario", "Usuario", user);
            sle.Insert(0, new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() });
            ViewBag.user = sle;

            return View(srlist);
        }


        public ActionResult CapturesReport(string user, Guid? idgroup, DateTime? Datefrom, DateTime? Dateto, int? hour)
        {
            List<CapturesViewModel> result = new List<CapturesViewModel>();
            Guid IdCompany = Guid.Empty;
            string IdCompany_ = IdCompany.ToString();
            try
            {
                IdCompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
                IdCompany_ = Request.RequestContext.HttpContext.Session["Company"].ToString();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Home");
            }

            if (Datefrom == null)
            {
                Datefrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 01, 01, 00);
            }

            if (Dateto == null)
            {
                Dateto = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 00);
            }
            else
                Dateto = Dateto.Value.AddHours(23).AddMinutes(59);


            if (Datefrom != null && Dateto != null)
            {
                List<string> users = new List<string>();

                if (idgroup != Guid.Empty && idgroup != null)
                {
                    users = new List<string>();
                    if (!string.IsNullOrEmpty(user) && user != Guid.Empty.ToString())
                        users.Add(user);
                    else
                        users = _repositorio.ListUsuarioArea(IdCompany, idgroup.Value).Select(s => s.Usuario).ToList();//db.Agent_Employee.Where(f => f.IdCompany == IdCompany).Select(s => s.Usuario).ToList(); //db.Agent_EmployeeGroupsEmployee.Where(f => f.Agent_EmployeesGroups.idemployeesGroup == idgroup).Select(g => g.Agent_Employee.Usuario).ToList();
                }

                if (!string.IsNullOrEmpty(user) && user != Guid.Empty.ToString() && idgroup == Guid.Empty && idgroup == null)
                {
                    users.Add(user);
                }


                IQueryable<CaptureBase> ListCapturesViewModel = (from e in MongoHelper.database.GetCollection<CaptureBase>("WindowsCapture").AsQueryable<CaptureBase>()
                                                                 where e.IdCompany == IdCompany_
                                                                 && (e.Date >= Datefrom.Value && e.Date <= Dateto.Value)
                                                                 select new CaptureBase
                                                                 {
                                                                     idrecord = e.idrecord,
                                                                     UserName = e.UserName,
                                                                     Image = e.Image,
                                                                     Date = e.Date,
                                                                     Hour = e.Hour
                                                                 });

                if (hour != null && hour != 0)
                    ListCapturesViewModel = ListCapturesViewModel.Where(t => t.Hour == hour);

                if (users.Count() > 0)
                    ListCapturesViewModel = ListCapturesViewModel.Where(p => users.Contains(p.UserName));

                foreach (var i in ListCapturesViewModel.OrderByDescending(o => o.Date).Take(100).ToList())
                {
                    CapturesViewModel cvm = new CapturesViewModel();
                    cvm.idrecord = i.idrecord;
                    cvm.UserName = i.UserName;
                    cvm.image = Convert.ToBase64String(i.Image);
                    cvm.Date = i.Date;
                    result.Add(cvm);
                }

                ViewBag.Datefrom = Datefrom.Value.ToString("yyyy-MM-dd");
                ViewBag.Dateto = Dateto.Value.ToString("yyyy-MM-dd");
            }
            List<SelectListItem> sli = CreateList(db.Agent_EmployeesGroups.Where(c => c.Agent_Empresa.IdCompany == IdCompany).ToList(), "idemployeesGroup", "Nombre", idgroup);
            sli.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.idgroup = sli;

            //List<SelectListItem> sle = CreateList(db.Agent_Employee.Where(c => c.IdCompany == IdCompany).ToList(), "Usuario", "Usuario", user);
            //sle.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            //ViewBag.user = sle;

            if (idgroup != null)
            {
                EmployeeSelectList(idgroup.Value);
            }


            return View(result.OrderByDescending(o => o.Date));
        }

        public ActionResult FileTransfer()
        {
            try
            {
                var users = new List<SelectListItem>();
                Guid idCompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
                users = db.Agent_Employee.Where(f => f.IdCompany == idCompany).Select(g => new SelectListItem { Text = g.Usuario, Value = g.Usuario }).ToList();
                users.Insert(0, (new SelectListItem { Text = "Seleccione", Value = "" }));
                ViewBag.user = users;
                return View();
            }
            catch (Exception)
            {
                return View();
            }
        }

        public ActionResult HistoryLocation()
        {
            try
            {
                if (!bool.Parse(System.Configuration.ConfigurationManager.AppSettings["VisibleGeolocation"]))
                    return RedirectToAction("MostUsedApps");

                var users = new List<SelectListItem>();
                Guid idCompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
                users = db.Agent_Employee.Where(f => f.IdCompany == idCompany).Select(g => new SelectListItem { Text = g.Usuario, Value = g.Usuario }).ToList();
                users.Insert(0, (new SelectListItem { Text = "Seleccione", Value = "" }));
                ViewBag.user = users;
                return View();
            }
            catch (Exception)
            {
                return View();
            }
        }

        public ActionResult ParameterSystem()
        {
            try
            {
                return View();
            }
            catch (Exception)
            {
                return View();
            }
        }

        [HttpGet]
        public JsonResult GetCapturesReport(string id_)
        {
            try
            {
                Guid IdCompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
                MongoHelper.UserCapture = MongoHelper.database.GetCollection<CaptureBase>("WindowsCapture");
                var builder = Builders<CaptureBase>.Filter;
                var filter = builder.Eq("idrecord", id_);

                CaptureBase Capturelist = new CaptureBase();
                Capturelist = MongoHelper.UserCapture.Find(filter).SingleOrDefault();

                CapturesViewModel cvm = new CapturesViewModel();
                if (Capturelist != null)
                {
                    cvm._id = Capturelist._id.ToString();
                    cvm.UserName = Capturelist.UserName;
                    cvm.idrecord = Capturelist.idrecord;
                    cvm.image = Convert.ToBase64String(Capturelist.Image);
                    cvm.Date = Capturelist.Date;
                }
                return Json(cvm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
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


        [HttpGet]
        public ActionResult TimePerActivity()
        {
            Guid company = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

            //List<SelectListItem> sliu = CreateList(db.Agent_Employee.Where(u => u.IdCompany == company), "Usuario", "Usuario").ToList();
            //sliu.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            //ViewBag.user = sliu;

            List<SelectListItem> sli = CreateList(db.Agent_EmployeesGroups.Where(c => c.Agent_Empresa.IdCompany == company).ToList(), "idemployeesGroup", "Nombre");
            sli.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.idgruoup = sli;

            TimePerActivityViewModel datos = new TimePerActivityViewModel();

            return View(datos);
        }

        public ActionResult TimePerActivity(TimePerActivityViewModel activity)
        {
            Guid company = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

            if (activity.from.Year <= 1900)
                activity.from = DateTime.Today;

            if (activity.to.Year <= 1900)
                activity.to = DateTime.Today;


            List<BasicStatsDashboard> data = GetDataForDashBoard(company.ToString(), activity.from, activity.to, activity.user, activity.idgruoup);
            TimePerActivityViewModel datos = new TimePerActivityViewModel();
            if (data.Count() > 0)
            {
                foreach (var i in data.GroupBy(g => g.Application))
                {
                    ActivitySumViewModel activities = new ActivitySumViewModel();

                    activities.program = i.Key;
                    double times = data.Where(b => b.Application == i.Key).Sum(k => k.Time).Value;

                    //sacamos minutos
                    times = times / 60;

                    //sacamos hotas
                    times = times / 60;

                    activities.time = times;

                    if (times > 0.01)
                        datos.activities.Add(activities);
                }
            }
            datos.activities = datos.activities.OrderByDescending(o => o.time).ToList();

            //List<SelectListItem> sliu = CreateList(db.Agent_Employee.Where(u => u.IdCompany == company), "Usuario", "Usuario", activity.user).ToList();
            //sliu.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            //ViewBag.user = sliu;

            if (activity.idgruoup != null)
            {
                EmployeeSelectList(activity.idgruoup);
            }

            List<SelectListItem> sli = CreateList(db.Agent_EmployeesGroups.Where(c => c.Agent_Empresa.IdCompany == company).ToList(), "idemployeesGroup", "Nombre", activity.idgruoup);
            sli.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.idgruoup = sli;

            return View(datos);
        }

        public ActionResult EmployeeLocation()
        {
            try
            {
                if (!bool.Parse(System.Configuration.ConfigurationManager.AppSettings["VisibleGeolocation"]))
                    return RedirectToAction("MostUsedApps");

                ViewBag.Title = "User Location";
                var _listItem = new List<SelectListItem>();
                Guid _idCompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
                var _users = db.Agent_Employee
                    .Where(f => f.IdCompany == _idCompany)
                    .Select(u => new GeoLocationModel
                    {
                        EmployeeId = u.idEmployee,
                        CityName = u.Ciudad,
                        CountryName = u.Pais,
                        RegionName = u.Region,
                        Ip = u.Ip,
                        Latitude = u.Latitud,
                        Longitude = u.Longitud,
                        Username = u.Usuario,
                    })
                    .ToList();
                _listItem = _users
                    .Select(g => new SelectListItem
                    {
                        Text = g.Username,
                        Value = g.EmployeeId.ToString()
                    })
                    .ToList();
                _listItem.Insert(0, (new SelectListItem { Text = "Todos", Value = "" }));
                ViewBag.ListItem = _listItem;
                ViewBag.users = _users;
                return View();
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult MostUsedApps()
        {
            try
            {
                var datos = new MostUsedAppsViewModel();
                Guid company = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

                //var allWorkAreaList = _repositorio.ListArea(company);
                //ViewData["AreasList"] = new SelectList(allWorkAreaList, "IdWorkArea", "WorkAreaName");
                FillGroupList(company, ref datos);
                datos.activities = new List<ActivitySumViewModel>();
                return View(datos);
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        /// <summary>
        /// It fills the DropDownList present in the Create view with the employee groups belonging to a company
        /// </summary>
        /// <param name="idcompany">
        /// The company id
        /// </param>
        public void FillGroupList(Guid idcompany, ref MostUsedAppsViewModel activity)
        {
            var allWorkAreaList = _repositorio.ListGroups(idcompany);
            ViewData["GroupList"] = new SelectList(allWorkAreaList, "idemployeesGroup", "Nombre", activity.IdEmployeesGroup);
        }


        public ActionResult MostUsedApps(MostUsedAppsViewModel activity)
        {
            Guid company = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

            if (!ModelState.IsValid)
            {

                FillGroupList(company, ref activity);


                if (activity.IdEmployeesGroup != null)
                {
                    EmployeeSelectList(activity.IdEmployeesGroup); 

                }


                activity.activities = new List<ActivitySumViewModel>();

                return View(activity);
            }

            List<BasicStatsDashboard> data = GetDataForMostUsedApps(company.ToString(), activity.from, activity.to, activity.user, activity.IdEmployeesGroup.ToString());

            if (data.Any())
            {
                foreach (var i in data.GroupBy(g => g.Application))
                {
                    ActivitySumViewModel activities = new ActivitySumViewModel();

                    activities.program = i.Key;
                    double times = data.Where(b => b.Application == i.Key).Sum(k => k.Time).Value;

                    //sacamos minutos
                    times = times / 60;

                    //sacamos hotas
                    times = times / 60;

                    activities.time = times;

                    if (times > 0.01)
                        activity.activities.Add(activities);
                }
            }


            FillGroupList(company, ref activity);

          

            if (activity.IdEmployeesGroup != null) {
                EmployeeSelectList(activity.IdEmployeesGroup);
            }

            return View(activity);
        }

        // ==================================== Paginas mas Buscadas


        public List<BasicStatsDashboard> GetDataForMostSearchedPages(string idcompany, DateTime from, DateTime to, string user, string IdWorkArea)
        {
            try
            {
                Guid idempresa = Guid.Parse(idcompany);
                Guid idWorkArea = Guid.Parse(IdWorkArea);
                List<Agent_ProgramClasification> clasifications = db.Agent_ProgramClasification.Where(t => t.Agent_Empresa.IdCompany == idempresa).ToList();

                var _startTest = new DateTime(from.Year, from.Month, from.Day);
                var _endTest = new DateTime(to.Year, to.Month, to.Day);
                _endTest = _endTest.Add(new TimeSpan(23, 59, 59));

                List<BasicStatsDashboard> queryPrincipal = new List<BasicStatsDashboard>();

                List<string> users = new List<string>();

                if (!string.IsNullOrEmpty(user) && user != Guid.Empty.ToString() && user != "Todos")
                    users.Add(user.ToLower());
                else
                    users = db.WorkAreaEmployee.Where(f => f.IdWorkArea == idWorkArea).Select(g => g.employee.Usuario.ToLower()).ToList();

                queryPrincipal = MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>().
                    Where(e => e.IdEmpresa == idcompany && (e.FocusTime >= _startTest && e.FocusTime <= _endTest))
                    .Select(e =>
                               new BasicStatsDashboard
                               {
                                   User = e.UserName,
                                   Application = e.Application,
                                   Time = e.Activity,
                                   Date_ = e.Date,
                                   URL = e.Url // Assuming URL field exists
                               }).OrderBy(o => o.Date_).ToList();

                queryPrincipal = queryPrincipal.Where(e => users.Contains(e.User.ToLower())).ToList();

                return queryPrincipal;
            }
            catch (Exception ex)
            {
                return new List<BasicStatsDashboard>();
            }
        }

        [HttpGet]
        public ActionResult MostSearchedPages()
        {
            try
            {
                var datos = new MostSearchedPagesViewModel();
                Guid company = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

                //var allWorkAreaList = _repositorio.ListArea(company);
                //ViewData["AreasList"] = new SelectList(allWorkAreaList, "IdWorkArea", "WorkAreaName");
                FillGroupList(company, ref datos);
                datos.activities = new List<ActivitySumViewModel>();
                return View(datos);
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        /// <summary>
        /// It fills the DropDownList present in the Create view with the employee groups belonging to a company
        /// </summary>
        /// <param name="idcompany">
        /// The company id
        /// </param>
        public void FillGroupList(Guid idcompany, ref MostSearchedPagesViewModel activity)
        {
            var allWorkAreaList = _repositorio.ListGroups(idcompany);
            ViewData["GroupList"] = new SelectList(allWorkAreaList, "idemployeesGroup", "Nombre", activity.IdEmployeesGroup);
        }


        public ActionResult MostSearchedPages(MostSearchedPagesViewModel activity)
        {
            Guid company = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

            if (!ModelState.IsValid)
            {
                FillGroupList(company, ref activity);

                if (activity.IdEmployeesGroup != null)
                {
                    EmployeeSelectList(activity.IdEmployeesGroup);
                }

                activity.activities = new List<ActivitySumViewModel>();
                return View(activity);
            }

            List<BasicStatsDashboard> data = GetDataForMostSearchedPages(company.ToString(), activity.from, activity.to, activity.user, activity.IdEmployeesGroup.ToString());

            if (data.Any())
            {
                foreach (var i in data.GroupBy(g => g.URL))
                {
                    // Filtra las URLs vacías
                    if (!string.IsNullOrEmpty(i.Key))
                    {
                        ActivitySumViewModel activities = new ActivitySumViewModel
                        {
                            Url = i.Key,
                            time = i.Sum(b => b.Time ?? 0) / 3600 // Convertir tiempo a horas
                        };

                        if (activities.time > 0.01) // Puedes ajustar el umbral según sea necesario
                        {
                            activity.activities.Add(activities);
                        }
                    }
                }
            }

            FillGroupList(company, ref activity);

            if (activity.IdEmployeesGroup != null)
            {
                EmployeeSelectList(activity.IdEmployeesGroup);
            }

            return View(activity);
        }



        // ==================================== Fin 

        [HttpGet]
        public async Task<ActionResult> EmployeeLocationById(Guid userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }
                endDate = endDate.AddHours(23).AddMinutes(59);
                var _users = await (from
                               ag in db.Agent_Employee
                                    join
                                    ul in db.UserLocation on ag.idEmployee equals ul.IdEmployee
                                    where
                                         ag.idEmployee == userId &&
                                         ul.Fecha >= startDate &&
                                         ul.Fecha <= endDate
                                    select new GeoLocationModel
                                    {
                                        EmployeeId = ag.idEmployee,
                                        Username = ag.Usuario,
                                        CityName = ul.CityName,
                                        CountryName = ul.CountryName,
                                        RegionName = ul.RegionName,
                                        Ip = ul.Ip,
                                        Latitude = ul.Latitude,
                                        Longitude = ul.Longitude,
                                        Date = ul.Fecha
                                    })
                               .ToListAsync();

                return Json(_users, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public ActionResult NotificacionUsuarioReport()
        {
            ICollection<NotificacionUsuarioReport> notificaciones = new List<NotificacionUsuarioReport>();
            Guid IdCompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
            if (IdCompany != Guid.Empty)
            {

                var resultadosDesdeBD = db.Agent_NotificacionUsuarios
                    .Where(x => x.status && x.IdCompany == IdCompany)
                    .Select(x => new
                    {
                        Usuario = x.Usuario,
                        Descripcion = x.Descripcion,
                        Date = x.Date
                    })
                .ToList();

                notificaciones = resultadosDesdeBD
                    .Select(x => new NotificacionUsuarioReport
                    {
                        Usuario = x.Usuario,
                        Descripcion = x.Descripcion,
                        Date = x.Date.ToString("dd/MM/yyyy HH:mm:ss")
                    })
                .ToList();

            }

            return View(notificaciones);
        }

        #endregion

        #region Alerts

        public void alert()
        {
            //primero borramos las alertas del dia anterior
            db.Alerts.RemoveRange(db.Alerts.Where(g => g.date < DateTime.Today).ToList());
            db.SaveChanges();

            MongoHelper.TrakerBase = MongoHelper.database.GetCollection<TrakerBase>("TrackerTime");
            var builder = Builders<TrakerBase>.Filter;

            List<Alertas> _alertas = db.Alertas.ToList();
            //var filter = builder.Gte("Date", DateTime.Today.AddDays(-5));
            var filter = builder.Gte("Date", DateTime.Today);

            var results = MongoHelper.TrakerBase.Find(filter).ToList();

            List<Agent_ProgramClasification> clasif = db.Agent_ProgramClasification.ToList();
            List<AlertsDataViewModel> lavm = new List<AlertsDataViewModel>();

            foreach (var i in results.GroupBy(g => g.IdEmpresa))
            {
                foreach (var j in results.Where(t => t.IdEmpresa == i.Key).GroupBy(g => g.UserName))
                {
                    foreach (var l in results.Where(t => t.IdEmpresa == i.Key && t.UserName == j.Key).GroupBy(a => a.Application))
                    {
                        AlertsDataViewModel avm = new AlertsDataViewModel();
                        Guid idempresa = Guid.Parse(i.Key);
                        avm.idempresa = i.Key;
                        avm.username = j.Key;
                        avm.application = l.Key;
                        avm.activity = results.Where(t => t.IdEmpresa == i.Key && t.UserName == j.Key && t.Application == l.Key).Sum(r => r.Activity);
                        avm.inactivity = results.Where(t => t.IdEmpresa == i.Key && t.UserName == j.Key && t.Application == l.Key).Sum(r => r.Inactivity).Value;
                        avm.clasification = clasif.Where(c => c.Agent_Empresa.IdCompany == idempresa && c.name == l.Key).Select(v => v.clasification).FirstOrDefault();
                        lavm.Add(avm);
                    }
                }
            }

            if (lavm.Count() > 0)
            {
                //clasificaciones
                // 1 prodictivas
                // 2 improductivas
                // 3 neutrales
                //

                // TIPOS DE ALERTAS
                // 2: APLIACIONES IMPRODUCTIVAS CON MAS DE 30 MIN DE USO
                // 3: MAS DE 30 MIN DE INACTIVIDAD ACUMULADA
                // 4: NO REPORTA DESDE HACE MAS DE 30 MIN

                foreach (var n in lavm.GroupBy(f => f.idempresa))
                {
                    foreach (var m in lavm.Where(t => t.idempresa == n.Key).GroupBy(u => u.username))
                    {
                        //sacamos la cantidad de tiempo de aplicaciones impoductivas de cada usuario
                        double improductivity = lavm.Where(p => p.username == m.Key && p.clasification == 2).Sum(s => s.activity);
                        Guid _idempresa = Guid.Parse(n.Key);

                        //alerta de usuario con mas de 30 min de uso en aplicaciones improductivas
                        if (improductivity >= 30 && db.Alerts.Where(r => r.tipo == 2 && r.Agent_Employee.Usuario == m.Key && r.Agent_Employee.IdCompany == _idempresa).Count() == 0)
                        {
                            Alerts a = new Alerts();
                            a.IdAlerts = Guid.NewGuid();
                            a.Agent_Employee = db.Agent_Employee.Where(d => d.IdCompany == _idempresa && d.Usuario == m.Key).SingleOrDefault();
                            a.tipo = 2;
                            a.status = false;
                            a.date = DateTime.Now;
                            db.Alerts.Add(a);
                        }

                        double inactivity = lavm.Where(p => p.username == m.Key).Sum(s => s.inactivity);

                        //Alerts alla = db.Alerts.Where(r => r.tipo == 3 && r.Agent_Employee.Usuario == m.Key && r.Agent_Employee.IdCompany == _idempresa).SingleOrDefault();

                        //alerta de usuario con mas de 30 min de inactividad acumulada
                        if (inactivity >= 30 && db.Alerts.Where(r => r.tipo == 3 && r.Agent_Employee.Usuario == m.Key && r.Agent_Employee.IdCompany == _idempresa).Count() == 0)
                        {
                            Alerts a = new Alerts();
                            a.IdAlerts = Guid.NewGuid();
                            a.Agent_Employee = db.Agent_Employee.Where(d => d.IdCompany == _idempresa && d.Usuario == m.Key).SingleOrDefault();
                            a.tipo = 3;
                            a.status = false;
                            a.date = DateTime.Now;
                            db.Alerts.Add(a);
                        }


                        // no reporta desde hace mas de 30 minE

                        //nos traemos la ultima vez que reporto
                        DateTime lastreport = results.Where(p => p.UserName == m.Key && p.IdEmpresa == n.Key).OrderByDescending(o => o.Date).Select(f => f.Date).FirstOrDefault();

                        //sacamos la cuenta de cuantos minutos han pasado desde su ultimo reporte
                        var lastreportmin_ = (DateTime.Now - lastreport).TotalMinutes;

                        //si mi ultimo reporte fue hace mas de 30 min, entonces alerto
                        if (lastreportmin_ >= 30 && db.Alerts.Where(r => r.tipo == 4 && r.Agent_Employee.Usuario == m.Key && r.Agent_Employee.IdCompany == _idempresa).Count() == 0)
                        {
                            Alerts a = new Alerts();
                            a.IdAlerts = Guid.NewGuid();
                            a.Agent_Employee = db.Agent_Employee.Where(d => d.IdCompany == _idempresa && d.Usuario == m.Key).SingleOrDefault();
                            a.tipo = 4;
                            a.status = false;
                            a.date = DateTime.Now;
                            db.Alerts.Add(a);
                        }
                        else
                        {
                            //si no consigue nada tratamos de borrar ya que ahora si esta reportando
                            db.Alerts.RemoveRange(db.Alerts.Where(r => r.tipo == 4 && r.Agent_Employee.Usuario == m.Key && r.Agent_Employee.IdCompany == _idempresa).ToList());
                        }
                    }

                    db.SaveChanges();

                    ///DESPUES QUE HACEMOS LAS VALIDACIONES, BUSCAMOS LO QUE HAY QUE REPORTAR PARA ENVIAR LOS CORREOS

                    List<Alerts> Alerts_ = db.Alerts.Where(t => t.status == false).Include(a => a.Agent_Employee).ToList();
                    foreach (var c in Alerts_.Where(t => t.Agent_Employee != null).GroupBy(g => g.Agent_Employee.IdCompany))
                    {
                        Agent_Empresa empr = db.Agent_Empresa.Where(e => e.IdCompany == c.Key).SingleOrDefault();
                        List<string> users_ = new List<string>();
                        foreach (var d in Alerts_.Where(t => t.Agent_Employee != null && t.Agent_Employee.IdCompany == c.Key).GroupBy(l => l.tipo))
                        {
                            users_ = new List<string>();
                            foreach (var e in Alerts_.Where(t => t.Agent_Employee != null && t.Agent_Employee.IdCompany == c.Key && t.tipo == d.Key))
                            {
                                users_.Add(e.Agent_Employee.Usuario);
                            }
                            List<string> _AlertAsociados = new List<string>();
                            if (users_.Count() > 0)
                            {
                                //traemos la listade correos por el tipo de alerta
                                Guid alertid = _alertas.Where(g => g.type == d.Key).Select(j => j.Id).SingleOrDefault();
                                _AlertAsociados = db.AlertAsociados.Where(t => t.Alertas.Id == alertid && t.Agent_Empresa.IdCompany == c.Key).Select(p => p.Email).ToList();

                                if (_AlertAsociados.Count() > 0)
                                {
                                    EmailController ec = new EmailController();
                                    //ec.SendAlert(_AlertAsociados, d.Key, users_);
                                }
                            }

                            if (_AlertAsociados.Count() > 0)
                            {
                                List<Alerts> laedit = db.Alerts.Where(h => h.Agent_Employee.IdCompany == c.Key && h.tipo == d.Key).ToList();

                                foreach (var o in laedit)
                                {
                                    o.status = true;
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Alertas(Guid? Id)
        {
            AlertModuleViewModel avm = new AlertModuleViewModel();
            var company = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

            List<AlertAsociados> oAlerta = new List<AlertAsociados>();
            if (Id != null && Id != Guid.Empty)
            {
                oAlerta = db.AlertAsociados.Where(p => p.Alertas.Id == Id && p.IdCompany == company).Include(a => a.Alertas).ToList();
                foreach (var j in oAlerta)
                {
                    AlertListViewModel alaso = new AlertListViewModel();
                    alaso.id = j.Id;
                    alaso.alert = j.Alertas.Alerta;
                    alaso.mail = j.Email;
                    avm.AlertListViewModel.Add(alaso);
                }
            }

            List<SelectListItem> alert = CreateList(db.Alertas.ToList(), "Id", "Alerta");
            alert.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.Id = alert;

            return View(avm);

        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Alertas(AlertModuleViewModel alert)
        {
            if (ModelState.IsValid)
            {
                var company = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

                if (alert.id != null && !string.IsNullOrEmpty(alert.AlertAsociados?.Email))
                {
                    Guid idalet = Guid.Parse(alert.id);

                    if (db.AlertAsociados.Where(t => t.IdCompany == company && t.Email == alert.AlertAsociados.Email && t.Alertas.Id == idalet).Count() == 0)
                    {
                        alert.AlertAsociados.Id = Guid.NewGuid();
                        alert.AlertAsociados.IdCompany = company;
                        alert.AlertAsociados.Alertas = db.Alertas.Where(x => x.Id == idalet).SingleOrDefault();
                        db.AlertAsociados.Add(alert.AlertAsociados);
                        db.SaveChanges();
                        Success("Registro exitoso");
                    }
                    else
                        Warning("Email ya se encuentra reacionado con esta alerta", string.Empty);
                }
            }

            List<SelectListItem> lalert = CreateList(db.Alertas.ToList(), "Id", "Alerta", alert.AlertAsociados?.Id);
            lalert.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.Id = lalert;

            return RedirectToAction("Alertas", new { Id = alert.id });
        }

        [HttpPost]
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AlertAsociados alertAsociados = db.AlertAsociados.Find(id);
            if (alertAsociados == null)
            {
                return HttpNotFound();
            }
            else
            {
                db.AlertAsociados.Remove(alertAsociados);
                db.SaveChanges();
                Success("Elimado exitosamente");
                return RedirectToAction("Alertas");
            }

        }

        #endregion
    }
}