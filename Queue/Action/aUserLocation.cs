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
using Queue.DAL;
using Queue.Models.ResponseDTO;
using Queue.Action.Mapper;
using System.Security.Cryptography.X509Certificates;
using DnsClient.Protocol;

namespace Queue.Action
{
    public class aUserLocation
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        aUtilities autil = new aUtilities();
        ClaimsPrincipal claims = new ClaimsPrincipal();
        TokenValidationHandler tokenValidation = new TokenValidationHandler();

        public object UserLocation(LocationModel model)
        {
            SaveDebugLog(model);
            Response response = new Response();
            Guid empresaId = Guid.Parse(model.IdEmpresa.ToString());
            try
            {
                claims = tokenValidation.getprincipal(Convert.ToString(model.Token));
                if (claims != null)
                {
                    if (model.LocationViewModel != null)
                    {
                        using (QueueContext ent = new QueueContext())
                        {
                            var empoyee = ent.Agent_Employee.Where(r => r.Usuario == model.User && r.IdCompany == empresaId).FirstOrDefault();
                            if (empoyee == null)
                            {
                                response.response_code = GenericErrors.GeneralError.ToString();
                                response.message = $"El Empleado con nombre de usuario {model.User} no existe.";
                                return response;
                            }

                            //Actualiza Ubicación
                            empoyee.Ip = model.LocationViewModel.Ip;
                            empoyee.CodigoPais = model.LocationViewModel.CountryCode;
                            empoyee.Pais = model.LocationViewModel.CountryName;
                            empoyee.Region = model.LocationViewModel.RegionName;
                            empoyee.Ciudad = model.LocationViewModel.CityName;
                            empoyee.Latitud = model.LocationViewModel.Latitude;
                            empoyee.Longitud = model.LocationViewModel.Longitude;                            
                            ent.SaveChanges();

                            //Inserta Historico
                                var newLocation = new UserLocation
                                {
                                    Id =  Guid.NewGuid(),
                                    IdEmployee = empoyee.idEmployee,
                                    IdCompany = empresaId,
                                    Ip = model.LocationViewModel.Ip,
                                    CountryCode = model.LocationViewModel.CountryCode,
                                    CountryName = model.LocationViewModel.CountryName,
                                    RegionName = model.LocationViewModel.RegionName,
                                    CityName = model.LocationViewModel.CityName,
                                    Latitude = model.LocationViewModel.Latitude,
                                    Longitude = model.LocationViewModel.Longitude,
                                    ZipCode = model.LocationViewModel.ZipCode,
                                    TimeZone = model.LocationViewModel.TimeZone,
                                    MobileBrand = model.LocationViewModel.MobileBrand,
                                    Elevation = model.LocationViewModel.Elevation,
                                    Fecha = DateTime.UtcNow.AddHours(-5)
                                };
                                ent.UserLocation.Add(newLocation);
                                ent.SaveChanges();

                        }
                        response.response_code = GenericErrors.SaveOk.ToString();
                    }
                
                 }
                else
                {
                    //token invalido
                    response = autil.ReturnMesagge(ref response, (int)GenericErrors.InvalidToken, string.Empty, null, HttpStatusCode.OK);
                    SaveDebugLog(response);
                    return response;
                }
                SaveDebugLog(response);
                return response;
            }
            catch (Exception ex)
            {
                SaveErrorLog(ex.Message + " " + ex.InnerException, ex);
                response = autil.ReturnMesagge(ref response, (int)GenericErrors.GeneralError, ex.Message + " " + ex.InnerException, null, HttpStatusCode.InternalServerError);
                return response;
            }
        }

        public object Validate(LocationModel model)
        {
            SaveDebugLog(model);
            Response response = new Response();
            try
            {
                claims = tokenValidation.getprincipal(Convert.ToString(model.Token));
                if (claims != null)
                {
                    if (model.LocationViewModel != null)
                    {
                        using (QueueContext ent = new QueueContext())
                        {
                            var company = ent.Agent_Empresa.Where(x=> x.Key == model.IdEmpresa).FirstOrDefault();                           
                            if (company == null) {
                                response.response_code = GenericErrors.GeneralError.ToString();
                                response.message = $"La empresa con key {model.IdEmpresa} no existe.";
                                return response;
                            }
                            Guid empresaId = Guid.Parse(company.IdCompany.ToString());
                            var empoyee = ent.Agent_Employee.Where(r => r.Usuario == model.User && r.IdCompany == empresaId).FirstOrDefault();
                            if (empoyee == null)
                            {
                                response.response_code = GenericErrors.GeneralError.ToString();
                                response.message = $"El Empleado con nombre de usuario {model.User} no existe para la compañía {company.Nombre}.";
                                return response;
                            }                          
                            DateTime fechaActual = DateTime.UtcNow.AddHours(-5);
                            var validateSesion = ent.UserLocation.Where(x=> x.IdCompany == empresaId && x.IdEmployee == empoyee.idEmployee).OrderByDescending(x => x.Fecha).FirstOrDefault();
                            //var validateSesionPrueba = (fechaActual - validateSesion.Fecha).TotalMinutes <= 5;
                            if (((fechaActual - validateSesion.Fecha).TotalMinutes > 5))
                            {
                                response.response_code = GenericErrors.GeneralError.ToString();
                                response.message = $"El Empleado con nombre de usuario {model.User} no ha validado su ubicación.";
                                return response;
                            }
                        }
                        response.response_code = HttpStatusCode.OK.ToString();
                        response.message = "Ok";
                    }

                }
                else
                {
                    //token invalido
                    response = autil.ReturnMesagge(ref response, (int)GenericErrors.InvalidToken, string.Empty, null, HttpStatusCode.OK);
                    SaveDebugLog(response);
                    return response;
                }
                SaveDebugLog(response);
                return response;
            }
            catch (Exception ex)
            {
                SaveErrorLog(ex.Message + " " + ex.InnerException, ex);
                response = autil.ReturnMesagge(ref response, (int)GenericErrors.GeneralError, ex.Message + " " + ex.InnerException, null, HttpStatusCode.InternalServerError);
                return response;
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