using Microsoft.AspNet.SignalR.Messaging;
using MongoDB.Driver;
using Queue.DAL;
using Queue.Models;
using Queue.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Queue.DataBase
{
    public class Repositorio : IRepositorio
    {
        private QueueContext db = new QueueContext();

        public List<FunctionalityViewModel> ListFunctionality(string companyId)
        {
            try
            {
                Guid idCompany = Guid.Parse(companyId);

                var fListViewModel = db.CompanyFunctionality.Where(
                    f => f.IdCompany.CompareTo(idCompany) == 0)
                    .Select(f => new FunctionalityViewModel() { Key = f.Functionality.Name }).ToList();

                return fListViewModel;


            } catch {
                return null;
            }
        }
      

        public List<ProgramsLicensedActivityViewModel> GetDataForInstalledVsLicensed(string idcompany, DateTime from, DateTime to, string user, string IdWorkArea)
        {
            try
            {
                Guid IdCompany = Guid.Parse(idcompany);
                Guid idWorkArea = Guid.Parse(IdWorkArea);
                var dataPrincipalUsed = new List<ActivitySumViewModel>();
                var dataPrincipalInstall = new List<ProgramsReport>();

                //Programs most used
                var clasifications = db.LicensePrograms.Join(db.Agent_ProgramClasification,
                            program => program.idprogramclasification,
                            license => license.idprogramclasification,
                            (license, program) => new { license, program })
                            .Where(t => t.program.Agent_Empresa.IdCompany == IdCompany).ToList();

                var _startTest = new DateTime(from.Year, from.Month, from.Day);
                var _endTest = new DateTime(to.Year, to.Month, to.Day);
                _endTest = _endTest.Add(new TimeSpan(23, 59, 59));

                List<string> users = new List<string>();

                if (!string.IsNullOrEmpty(user) && user != Guid.Empty.ToString() && user != "Todos")
                    users.Add(user.ToLower());
                else
                    users = db.WorkAreaEmployee.Where(f => f.IdWorkArea == idWorkArea).Select(g => g.employee.Usuario.ToLower()).ToList();

                var queryPrincipal = MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>().
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

                if (queryPrincipal?.Any() ?? false)
                {
                    var query = queryPrincipal.GroupBy(g => g.Application).ToList();
                    foreach (var i in query)
                    {
                        ActivitySumViewModel activities = new ActivitySumViewModel();

                        activities.program = i.Key;
                        double times = queryPrincipal.Where(b => b.Application == i.Key).Sum(k => k.Time).Value;

                        times /= 60;

                        times /= 60;

                        activities.time = times;
                        activities.workArea = idWorkArea;
                        activities.User = user;

                        var licenciada = clasifications.Where(l => l.program.name == i.Key).Select(l => new { l.license.isLicensed, l.license.licenseNumber }).FirstOrDefault();
                        activities.licensed = licenciada?.isLicensed ?? false;
                        activities.codigoLicensed = licenciada?.licenseNumber??"";

                        int numUser = i.GroupBy(g => g.User).Count();
                        activities.quantity = numUser;

                        if (times > 0.01)
                            dataPrincipalUsed.Add(activities);
                    }
                }
                //dataPrincipalUsed = dataPrincipalUsed.OrderByDescending(o => o.time).ToList();

                //Programs Installed

                MongoHelper.SoftWareList = MongoHelper.database.GetCollection<InstalledProgramsViewModel>("Software");
                var builder = Builders<InstalledProgramsViewModel>.Filter;
                var filter = builder.Eq("IdCompany", IdCompany) & builder.Eq("Status", true);

                List<InstalledProgramsViewModel> results = MongoHelper.SoftWareList.Find(filter).ToList();

                results = results.Where(u => users.Contains(u.User)).ToList();

                ProgramsReport sr;
                foreach (var i in results.GroupBy(g => g.Name))
                {
                    sr = new ProgramsReport();
                    sr.program = i.Key;
                    sr.quantity = i.Count();
                    if (!string.IsNullOrEmpty(user) && user != "Todos")
                        sr.user = user;
                    if (idWorkArea != Guid.Empty)
                        sr.workArea = idWorkArea;

                    var licenciada = clasifications.Where(l => l.program.name == i.Key).Select(l => new { l.license.isLicensed, l.license.licenseNumber }).FirstOrDefault();
                    sr.isLicensed = licenciada?.isLicensed ?? false;
                    sr.licenseNumber = licenciada?.licenseNumber??"";

                    dataPrincipalInstall.Add(sr);
                }

                //union apps instaled and apps used
                var aplicationsUsedNotInstallApp = dataPrincipalInstall.Select(d => d.program).ToList();
                var aplicationsInstallNotUsedApp = dataPrincipalUsed.Select(d => d.program).ToList();

                var aplicationsUsedNotInstall = dataPrincipalUsed.Where(d => !aplicationsUsedNotInstallApp.Contains(d.program))
                    .Select(d => new ProgramsLicensedActivityViewModel {
                        WorkArea = idWorkArea,
                        program = d.program,
                        time = d.time,
                        isUsed = true,
                        isInstalled = false,
                        numUser = d.quantity,
                        isLicensed = d.licensed,
                        License = d.codigoLicensed
                    }).ToList();
                var aplicationsInstallNotUsed = dataPrincipalInstall.Where(d => !aplicationsInstallNotUsedApp.Contains(d.program))
                    .Select(d => new ProgramsLicensedActivityViewModel
                    {
                        WorkArea = idWorkArea,
                        program = d.program,
                        time = 0,
                        isUsed = false,
                        isInstalled = true,
                        numUser = d.quantity,
                        isLicensed = d.isLicensed,
                        License = d.licenseNumber
                    }).ToList();
                var aplicationsInstallUsed = dataPrincipalInstall.Join(dataPrincipalUsed, used => used.program, install => install.program, (install, used) => new { install, used})
                    .Select(d => new ProgramsLicensedActivityViewModel
                    {
                        WorkArea = idWorkArea,
                        program = d.used.program,
                        time = d.used.time,
                        isUsed = true,
                        isInstalled = true,
                        numUser = d.install.quantity,
                        isLicensed = d.install.isLicensed,
                        License = d.install.licenseNumber
                    }).ToList();

                return aplicationsUsedNotInstall.Union(aplicationsInstallNotUsed).Union(aplicationsInstallUsed).ToList();

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<WorkArea> ListArea(Guid companyId) {
            return db.WorkArea.Where(c => c.IdCompany == companyId).ToList();
        }

        public List<Agent_EmployeesGroups> ListGroups(Guid companyId)
        {
            return db.Agent_EmployeesGroups.Where(c => c.Agent_Empresa.IdCompany == companyId).ToList(); 
        }

        public List<Agent_Employee> ListUsuarioArea(Guid companyId, Guid idWorkArea)
        {
            var QueryUserbyArea = (from t in db.Agent_Employee
                                   join r in db.WorkAreaEmployee on t.idEmployee equals r.idEmployee
                                   where t.IdCompany == companyId
                                   where r.IdWorkArea == idWorkArea
                                   select t).ToList();
            return QueryUserbyArea;
        }

        public bool AddFileTranfer(List<FileTransferViewModel> model)
        {
            try
            {
                MongoHelper.FileTransfer = MongoHelper.database.GetCollection<FileTransferViewModel>("FileTransfer");
                MongoHelper.FileTransfer.InsertMany(model);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
    }
}