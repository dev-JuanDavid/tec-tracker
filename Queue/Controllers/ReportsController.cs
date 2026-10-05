using Queue.Action;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Queue.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ReportsController : Controller
    {
        #region private readonly & static fields

        private OperationController operation;
        private aFileTransfer aFile;

        #endregion

        #region C´tor

        private OperationController _operation => operation ?? (operation = new OperationController());
        private aFileTransfer _aFile => aFile ?? (aFile = new aFileTransfer());

        #endregion

        #region Views

        public ActionResult FileTransfer()
        {
            try
            {
                var _users = _operation.GetActivityUser(Session["Company"].ToString());
                _users.Insert(0, (new SelectListItem { Text = "Seleccione", Value = "" }));
                ViewBag.user = _users;
                return View();
            }
            catch (Exception)
            {
                return View();
            }
        }

        #endregion

        #region Methods

        public async Task<JsonResult> GetFileTransfer(DateTime startDate,
                                                      DateTime endDate,
                                                      string user)
        {
            try
            {
                var _resService = _aFile.ReportFileTransfer(Session["Company"].ToString(), user, startDate, endDate);

                return Json(_resService.Select(ft => new
                {
                    ft.ProcessServices,
                    ft.MachineName,
                    ft.ProcessPath,
                    ft.FileName,
                    ProcessId = ft.ProcessID,
                    ft.ProcessName,
                    Username = ft.UserName,
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

        #endregion
    }
}