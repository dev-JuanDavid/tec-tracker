using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Claims;
using Queue.Controllers.Token;
using Queue.Models;
using static Queue.Common.CommonEnum;
using Queue.DAL;
using Queue.ViewModels;
using Queue.Controllers;
using System.Globalization;
using System.Threading;
using log4net;

namespace Queue.Action
{
    public class aAutomaticTakeTime
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        aUtilities autil = new aUtilities();
        ClaimsPrincipal cp = new ClaimsPrincipal();
        TokenValidationHandler tvh = new TokenValidationHandler();
        OperationController opc = new OperationController();

        //CREA LA ACTIVIDAD QUE EL USUARIO ESTA REALIZANDO
        public object CreateAutomaticTakeTime(TrackerModel o)
        {
            SaveDebugLog(o);

            Response rp = new Response();
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            try
            {
                cp = tvh.getprincipal(Convert.ToString(o.token));
                if (cp != null)
                {
                    List<TrakerBase> ltb = new List<TrakerBase>();

                    foreach (var i in o.AutomaticTakeTimeModel)
                    {
                        TrakerBase tb = new TrakerBase();
                        Copier.CopyPropertiesTo(i, tb);
                        ltb.Add(tb);
                    }

                    if (ltb.Count() > 0)
                    {
                        bool response = opc.AddTracker(ltb);
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
                //error general
                SaveErrorLog(ex.Message + " " + ex.InnerException, ex);
                rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, ex.Message + " " + ex.InnerException, null, HttpStatusCode.InternalServerError);
                return rp;
            }
        }

        //CREA CAPTURAS DE PANATALLA DE LOS USUARIOS UNA A UNA
        public object CreateCapture(CaptureModel o)
        {
            SaveDebugLog(o);
            Response rp = new Response();
            try
            {
                cp = tvh.getprincipal(Convert.ToString(o.token));
                if (cp != null)
                {
                    CaptureBase cb = new CaptureBase();
                    Copier.CopyPropertiesTo(o, cb);
                    if (cb != null)
                    {
                        bool response = opc.AddCapture(cb);
                        if (response)
                            rp.response_code = GenericErrors.SaveOk.ToString();
                        else
                            rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, string.Empty, null, HttpStatusCode.InternalServerError);

                        List<CaptureBase> getimage = opc.GeImage(o.IdCompany);
                    }

                }
            }
            catch (Exception ex)
            {
                SaveErrorLog(System.Reflection.MethodBase.GetCurrentMethod().Name+ " --- " + ex.Message + " " + ex.InnerException, ex);
                //error general
                rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, ex.Message + " " + ex.InnerException, null, HttpStatusCode.InternalServerError);
                return rp;
            }

            SaveDebugLog(rp);
            return rp;
        }

        //CREA CAPTURAS DE PANATALLA DE LOS USUARIOS MASIVAS
        public object CreateCaptures(List<CaptureModel> captureModel)
        {
            SaveDebugLog(captureModel);
            var token  = captureModel.Select(x=> x.token).FirstOrDefault();
            var companyId = captureModel.Select(x => x.IdCompany).FirstOrDefault();
            Response rp = new Response();
            try
            {
                cp = tvh.getprincipal(Convert.ToString(token));
                if (cp != null)
                {
                    var cb = new List<CaptureBase>();
                    foreach (var capture in captureModel)
                    {
                        var cbCapture = new CaptureBase();
                        Copier.CopyPropertiesTo(capture, cbCapture);
                        cb.Add(cbCapture);
                    }

                    if (cb.Any())
                    {
                        bool response = opc.AddCaptures(cb);
                        if (response)
                            rp.response_code = GenericErrors.SaveOk.ToString();
                        else
                            rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, string.Empty, null, HttpStatusCode.InternalServerError);

                        List<CaptureBase> getimage = opc.GeImage(companyId);
                    }

                }
            }
            catch (Exception ex)
            {
                SaveErrorLog(System.Reflection.MethodBase.GetCurrentMethod().Name + " --- " + ex.Message + " " + ex.InnerException, ex);
                //error general
                rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, ex.Message + " " + ex.InnerException, null, HttpStatusCode.InternalServerError);
                return rp;
            }

            SaveDebugLog(rp);
            return rp;
        }

        //CREA LOS PROGRAMAS QUE TIENN LOS USUARIOS INSTALADOS EN SUS MAQUINAS, SI YA NO LOS TIENE LOS MARCA COMO DESACTIVADOS
        public object SaveIntalledPrograms(InstalledModel o)
        {
            SaveDebugLog(o);
            Response rp = new Response();
            try
            {
                cp = tvh.getprincipal(Convert.ToString(o.token));
                if (cp != null)
                {
                    string empresa = string.Empty;

                    List<InstalledProgramsViewModel> livm = new List<InstalledProgramsViewModel>();
                    foreach (var i in o.InstalledProgramsViewModel)
                    {
                        InstalledProgramsViewModel ivm = new InstalledProgramsViewModel();
                        Copier.CopyPropertiesTo(i, ivm);
                        empresa = i.IdCompany;
                        livm.Add(ivm);
                    };

                    if (livm.Count() > 0)
                    {
                        List<InstalledProgramsViewModel> installed = opc.GetSoftWare(empresa.ToString(), livm[0].Pc);
                        foreach (var i in installed)
                        {
                            //paso 1, validar las que ya estan en base de datos, para agregar las que no
                            InstalledProgramsViewModel ipvm = livm.Where(l => l.Name == i.Name && l.Vertion == i.Vertion).FirstOrDefault();
                            //si no esta en la lista que viene del monitor, es porque se desinstalo
                            if (ipvm == null)
                            {
                                opc.UpdateSotfware(i);
                            }
                            else
                            {
                                livm.Remove(ipvm);
                            }
                        }

                        if (livm.Count() > 0)
                        {
                            opc.AddSoftware(livm);
                        }
                        rp.response_code = GenericErrors.SaveOk.ToString();
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
            }
            catch (Exception ex)
            {
                SaveErrorLog(ex.Message + " " + ex.InnerException, ex);
                rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, ex.Message + " " + ex.InnerException, null, HttpStatusCode.InternalServerError);
                return rp;
            }
            SaveDebugLog(rp);
            return rp;
        }

        //GUARDA EL HARDWARE QUE TIENEN LOS USUARIOS, REVISA SI HAY NUEVOS Y LOS QUE YA NO, LOS MARCA COMO DESINSTALADOS
        public object SaveHardware(HardwareModel model)
        {
            SaveDebugLog(model);
            Response rp = new Response();
            try
            {
                cp = tvh.getprincipal(Convert.ToString(model.token));
                if (cp != null)
                {
                    string empresa = string.Empty;

                    List<InstalledHardwareViewModel> livm = new List<InstalledHardwareViewModel>();

                    foreach (var i in model.InstalledHardwareViewModel)
                    {
                        InstalledHardwareViewModel ivm = new InstalledHardwareViewModel();
                        empresa = i.IdCompany;
                        Copier.CopyPropertiesTo(i, ivm);
                        livm.Add(ivm);
                    };
                    if (livm.Count() > 0)
                    {
                        List<InstalledHardwareViewModel> installed = opc.GetHardware(empresa, livm[0].Pc);

                        foreach (var i in installed)
                        {
                            //paso 1, validar las que ya estan en base de datos, para agregar las que no
                            InstalledHardwareViewModel ipvm = livm.Where(l => l.Hardware == i.Hardware && l.Type == i.Type).SingleOrDefault();
                            //si no esta en la lista que viene del monitor, es porque se desinstalo
                            if (ipvm == null)
                            {
                                opc.UpdateHardware(i);
                            }
                            else
                            {
                                //si si esta, entonces la quito de la lista que viene del monitor para quedarme solo con las que no estan en BD que son las nuevas
                                livm.Remove(ipvm);
                            }
                        }

                        //si qedo algo lo agrego
                        if (livm.Count() > 0)
                        {
                            opc.AddHardware(livm);
                        }
                    }
                    rp.response_code = GenericErrors.SaveOk.ToString();
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
                //error general
                SaveErrorLog(ex.Message + " " + ex.InnerException, ex);
                rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, ex.Message + " " + ex.InnerException, null, HttpStatusCode.InternalServerError);
                return rp;
            }
            return rp;
        }


        //GUARDA LOGS, PARA HACER DEBUG D EERRORES O PARA GUARDAR LOGS DE LO QUE SE NECESITE
        public object Createlog(LogsViewModel o)
        {
            SaveDebugLog(o);
            Response rp = new Response();
            try
            {
                bool response = opc.AddLog(o);
                if (response)
                    rp.response_code = GenericErrors.SaveOk.ToString();
                else
                    rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, string.Empty, null, HttpStatusCode.InternalServerError);

                SaveDebugLog(rp);
                return rp;
            }
            catch (Exception ex)
            {
                //error general
                SaveErrorLog(ex.Message + " " + ex.InnerException, ex);
                rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, ex.Message + " " + ex.InnerException, null, HttpStatusCode.InternalServerError);
                return rp;
            }
        }

        public object GetHorary(HoraryModel hm)
        {
            Response rp = new Response();
            try
            {

                cp = tvh.getprincipal(Convert.ToString(hm.token));
                if (cp != null)
                {
                    using (QueueContext ent = new QueueContext())
                    {
                        Guid IdEmpresa = Guid.Parse(hm.idempresa);

                        Agent_Employee ege = ent.Agent_Employee.Where(r => r.IdCompany == IdEmpresa && r.Usuario.ToLower() == hm.user.ToLower()).Include(g => g.Agent_GroupHorary).SingleOrDefault();
                        if (ege != null)
                        {
                            if (ege.Agent_GroupHorary != null)
                            {
                                rp.HoraryModel.Agent_GroupHoraryDetail = ent.Agent_GroupHoraryDetail
                                                                        .Where(d => d.Agent_GroupHorary.Id_GroupHorary == ege.Agent_GroupHorary.Id_GroupHorary)
                                                                        .Select(p => new HoraryDetail
                                                                        {
                                                                            Day = p.Day,
                                                                            HourFrom = p.HourFrom,
                                                                            HourUntil = p.HourUntil,
                                                                            Type = p.Type
                                                                        }).ToList();

                            }
                        }
                    }
                    SaveDebugLog($"GetHorary -- Usuario: '{hm.user}'. Horarios: {rp.HoraryModel.Agent_GroupHoraryDetail.Count}");
                    //retorna un response, con el campo data lleno con la respuesta.
                    return autil.ReturnMesagge(ref rp, (int)GenericErrors.OK, null, null, HttpStatusCode.OK);
                }
                else
                {
                    //token invalido
                    rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.InvalidToken, string.Empty, null, HttpStatusCode.OK);
                    return rp;
                }
            }
            catch (Exception ex)
            {
                //error general
                SaveErrorLog(ex.Message + " " + ex.InnerException, ex);
                return autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, string.Empty, null, HttpStatusCode.InternalServerError);
            }
        }

        public void SaveDebugLog(object o, [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
        {
            _log.Info(Newtonsoft.Json.JsonConvert.SerializeObject( new DebugLogModel()
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