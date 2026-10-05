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
            SaveDebugLog(key);
            ///cambios del dia 23/03/2019
            using (QueueContext ent = new QueueContext())
            {
                object response = null;
                Response rp = new Response();
                try
                {
                    Agent_Empresa ae = ent.Agent_Empresa.Where(a => a.Key == key && a.status == true).SingleOrDefault();

                    if (ae != null)
                    {
                        if (ent.License.Where(c => c.Agent_Empresa.IdCompany == ae.IdCompany && c.enddate >= DateTime.Today).Count() > 0)
                        {
                            var token = tvh.GenerateToken(ae.Nombre, ae.Rut, ae.IdCompany.ToString());
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
                        }
                        else
                        {
                            response = autil.ReturnMesagge(ref rp, (int)GenericErrors.Licenceend, string.Empty, null);
                            SaveDebugLog(response);
                            return response;
                        }
                    }
                    else
                    {
                        //login invalido
                        response = autil.ReturnMesagge(ref rp, (int)GenericErrors.ErrorLogin, string.Empty, null);
                        SaveDebugLog(response);
                        return response;
                    }

                    //retorna un response, con el campo data lleno con la respuesta.
                    response = autil.ReturnMesagge(ref rp, (int)GenericErrors.OK, null, null, HttpStatusCode.OK);
                    SaveDebugLog(response);
                    return response;
                }
                catch (Exception ex)
                {
                    response = autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, string.Empty, null, HttpStatusCode.InternalServerError);
                    SaveDebugLog(response);
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