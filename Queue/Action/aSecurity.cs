using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using log4net;
using Queue.Action.Mapper;
using Queue.Controllers.Token;
using Queue.DAL;
using Queue.Models;
using Queue.Models.ResponseDTO;
using static Queue.Common.CommonEnum;

namespace Queue.Action
{
    public class aSecurity : AutoMapperBase
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        aUtilities autil = new aUtilities();
        ClaimsPrincipal cp = new ClaimsPrincipal();
        TokenValidationHandler tvh = new TokenValidationHandler();
        #region Login

        public object Login(object key)
        {
            var requestId = System.Web.HttpContext.Current == null ? null : System.Web.HttpContext.Current.Items["RequestLogId"];
            _log.InfoFormat("API Login: inicio; buscando empresa activa requestId={0}", requestId);
            ///cambios del dia 23/03/2019
            using (QueueContext ent = new QueueContext())
            {
                object response = null;
                Response rp = new Response();
                try
                {
                    Agent_Empresa ae = ent.Agent_Empresa.Where(a => a.Key == key && a.status == true).SingleOrDefault();
                    _log.InfoFormat("API Login: empresa encontrada={0} requestId={1}", ae != null, requestId);

                    if (ae != null)
                    {
                        _log.InfoFormat("API Login: comprobando licencia requestId={0}", requestId);
                        if (ent.License.Where(c => c.Agent_Empresa.IdCompany == ae.IdCompany && c.enddate >= DateTime.Today).Count() > 0)
                        {
                            var token = tvh.GenerateToken(ae.Nombre, ae.Rut, ae.IdCompany.ToString());
                            _log.InfoFormat("API Login: token generado; cargando configuración requestId={0}", requestId);
                            ResponseConfigurationDTO responseConfigurationDTO = (from c in ent.Agent_Configuration
                                                                                 where c.Agent_Empresa.IdCompany == ae.IdCompany
                                                                                 select new ResponseConfigurationDTO
                                                                                 {
                                                                                     Id_Configuration = c.Id_Configuration,
                                                                                     InactivityPeriod = c.InactivityPeriod,
                                                                                     UploadFrecuency = c.UploadFrecuency,
                                                                                     CaptureFrecuency = c.CaptureFrecuency,
                                                                                     LocationFrecuency = c.LocationFrecuency,
                                                                                     token = token,
                                                                                     IsLogged = true,
                                                                                     Id_Empresa = ae.IdCompany                                                                                     
                                                                                 }).FirstOrDefault();

                            rp.data = responseConfigurationDTO;
                            _log.InfoFormat("API Login: configuración encontrada={0} requestId={1}", responseConfigurationDTO != null, requestId);
                        }
                        else
                        {
                            response = autil.ReturnMesagge(ref rp, (int)GenericErrors.Licenceend, string.Empty, null);
                            _log.WarnFormat("API Login: licencia no vigente requestId={0}", requestId);
                            return response;
                        }
                    }
                    else
                    {
                        //login invalido
                        response = autil.ReturnMesagge(ref rp, (int)GenericErrors.ErrorLogin, string.Empty, null);
                        _log.WarnFormat("API Login: empresa no encontrada o inactiva requestId={0}", requestId);
                        return response;
                    }

                    //retorna un response, con el campo data lleno con la respuesta.
                    response = autil.ReturnMesagge(ref rp, (int)GenericErrors.OK, null, null, HttpStatusCode.OK);
                    _log.InfoFormat("API Login: respuesta OK requestId={0}", requestId);
                    return response;
                }
                catch (Exception ex)
                {
                    response = autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, string.Empty, null, HttpStatusCode.InternalServerError);
                    _log.Error(string.Format("API Login: excepción requestId={0}", requestId), ex);
                    return response;
                    //error general
                }
            }
        }
        #endregion

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
