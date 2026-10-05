using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Claims;
using Queue.Controllers.Token;
using Queue.Models;
using static Queue.Common.CommonEnum;
using Queue.ViewModels;
using Queue.Controllers;
using log4net;
using Queue.DataBase;

namespace Queue.Action
{
    public class aFileTransfer
    {
        private IRepositorio _repositorio;
        public aFileTransfer(){
            _repositorio = new Repositorio();
        }
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        aUtilities autil = new aUtilities();
        ClaimsPrincipal claims = new ClaimsPrincipal();
        TokenValidationHandler tokenValidation = new TokenValidationHandler();
        //OperationController operation = new OperationController();

        //CREA LOS PROCESOS QUE TIENN LOS USUARIOS CORRIENDO EN SUS MAQUINAS
        public object SaveFileTransfer(FileTransferModel model)
        {
            SaveDebugLog(model);
            Response rp = new Response();
            string empresa = model.IdEmpresa;
            string userName = model.User;
            try
            {
                claims = tokenValidation.getprincipal(Convert.ToString(model.Token));
                if (claims != null)
                {
                    List<FileTransferViewModel> process = new List<FileTransferViewModel>();

                    foreach (var i in model.FileTransferViewModel)
                    {
                        FileTransferViewModel dataModel = new FileTransferViewModel();
                        Copier.CopyPropertiesTo(i, dataModel);
                        dataModel.IdEmpresa = empresa;
                        dataModel.MachineName = i.MachineName;
                        dataModel.UserName = userName;
                        dataModel.Date = DateTime.UtcNow.AddHours(-5);
                        dataModel.FechaInsercion = DateTime.UtcNow.AddHours(-5).ToString("yyyy-MM-dd hh:mm tt");
                        process.Add(dataModel);
                    }

                    if (process.Count() > 0)
                    {
                        bool response = _repositorio.AddFileTranfer(process);

                        if (response)
                            rp.response_code = GenericErrors.SaveOk.ToString();
                        else
                            rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, string.Empty, null, HttpStatusCode.InternalServerError);
                    }
                }
                else
                {
                    //token invalido
                    rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.InvalidToken, string.Empty, null, HttpStatusCode.OK);
                    SaveDebugLog(rp);
                    return rp;
                }

                SaveDebugLog(rp);
                return rp;
            }

            catch (Exception ex)
            {
                SaveErrorLog(ex.Message + " " + ex.InnerException, ex);
                rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, ex.Message + " " + ex.InnerException, null, HttpStatusCode.InternalServerError);
                return rp;
            }
        }

        public List<FileTransferViewModel> ReportFileTransfer(string idCompany, string userName, DateTime startDate, DateTime endDate)
        {
            var model = JsonConvert.SerializeObject(new { userName, startDate, endDate });
            SaveDebugLog(model);

            try
            {
                List<FileTransferViewModel> result = _repositorio.ReportFileTransfer(idCompany, userName, startDate, endDate);

                SaveDebugLog(result);
                return result;
            }

            catch (Exception ex)
            {
                SaveErrorLog(ex.Message + " " + ex.InnerException, ex);
                return null;
            }
        }

        public void SaveDebugLog(object o, [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
        {
            _log.Info(Newtonsoft.Json.JsonConvert.SerializeObject(new DebugLogModel()
            {
                Method = caller,
                Message = o
            }));
        }
        public void SaveErrorLog(string message, Exception ex, [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
        {
            _log.Error(Newtonsoft.Json.JsonConvert.SerializeObject(new DebugLogModel()
            {
                Method = caller,
                Message = message
            }), ex);
        }
    }
}