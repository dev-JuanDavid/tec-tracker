using log4net;
using Newtonsoft.Json;
using Queue.Controllers.Token;
using Queue.DAL;
using Queue.DataBase;
using Queue.Models;
using Queue.Models.ResponseDTO;
using Queue.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Windows;
using static Queue.Common.CommonEnum;

namespace Queue.Action
{
    public class aFunctionality
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        aUtilities autil = new aUtilities();
        ClaimsPrincipal claims = new ClaimsPrincipal();
        TokenValidationHandler tokenValidation = new TokenValidationHandler();

        public Repositorio _repository;

        public aFunctionality()
        {
            _repository = new Repositorio();    
        }

        public HttpResponseMessage GetAllFunctionality(FunctionalityDTO model)
        {
            var token = model.Token;
            var companyId = model.CompanyId;
            SaveDebugLog(companyId);
            HttpResponseMessage rp = new HttpResponseMessage();

            try
            {
                claims = tokenValidation.getprincipal(Convert.ToString(token));
                if (claims != null)
                {
                    List<FunctionalityViewModel> models = _repository.ListFunctionality(companyId);
                    if (models != null && models.Any())
                    {
                        return autil.ReturnResponse(models);

                    } else
                    {
                        rp = ReturnMessage((int)GenericErrors.GeneralError, string.Empty, null, HttpStatusCode.NoContent);
                        SaveDebugLog(rp);
                        return rp;
                    }

                } else
                {
                    //token invalido
 
                    rp = ReturnMessage((int)GenericErrors.InvalidToken, string.Empty, null, HttpStatusCode.OK);
                    SaveDebugLog(rp);
                    return rp;

                }

            } catch (Exception ex)
            {
                SaveErrorLog(ex.Message + " " + ex.InnerException, ex);
                 return ReturnMessage((int)GenericErrors.GeneralError, ex.Message + " " + ex.InnerException, null, HttpStatusCode.InternalServerError);
                

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

        public HttpResponseMessage ReturnMessage(int idmensje, string custom, Guid? id, HttpStatusCode status = HttpStatusCode.OK) {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
           // StringContent content = new StringContent(JsonConvert.)

            using (QueueContext ent = new QueueContext())
            {
                try
                {
                    string codigo_ = idmensje.ToString();
                    Agent_GenericError ge = (from g in ent.Agent_GenericError
                                             where g.Codigo == codigo_
                    select g).SingleOrDefault();

                    httpResponseMessage.StatusCode = status;
                    string message = ge.Message + " " + custom;
                    httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(message, Formatting.Indented, new JsonSerializerSettings() { DateFormatString = "dd/MM/yyyy" }));
                    
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return httpResponseMessage;

        }
    }
}