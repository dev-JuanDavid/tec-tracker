using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using System.Net;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Queue.DAL;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using Queue.Models;

namespace Queue.Middleware
{
    public class AgentMiddlewareFilter : System.Web.Http.Filters.ActionFilterAttribute
    {
        private const string _NOTIFICACION_USUARIO_NO_REGISTRADO = "USUARIO NO REGISTRADO";

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            try
            {
                string requestBody = actionContext.Request.Properties["RequestBody"] as string;
                if (string.IsNullOrEmpty(requestBody))
                {
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    return;
                }

                string user         = string.Empty;
                string IdCompany    = string.Empty;

                // Evalua si el Contenido del Request Body es un Objeto o un Array de Objeto
                if (JsonConvert.DeserializeObject(requestBody) is JObject jsonObject)
                {
                    user        = jsonObject["user"]?.ToString();
                    IdCompany   = jsonObject["idempresa"]?.ToString();
                }
                else if (JsonConvert.DeserializeObject(requestBody) is JArray jsonArray)
                {    
                    foreach (JObject jsonBody in jsonArray.Children<JObject>())
                    {
                       user         = jsonBody["UserName"]?.ToString();
                       IdCompany    = jsonBody["IdCompany"]?.ToString();
                       break;
                    }
                }
                else
                {
                    // Retorna BadRequest En caso de que la estructura de datos no este definida en el Monitor Tracker Cliente
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    return;
                }

                if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(IdCompany))
                {
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    return;
                }

                var guidCompany = Guid.Parse(IdCompany);
                IEnumerable<Models.Agent_Employee> empleados = new List<Models.Agent_Employee>();
                using (QueueContext ent = new QueueContext())
                {
                    empleados = ent.Agent_Employee
                    .Where(e => e.IdCompany == guidCompany && e.Usuario.ToUpper() == user.ToUpper())
                    .ToList();
                }

                if (empleados.Count() == 0)
                {

                    NotificarUsuarioNoRegistrado(user.ToUpper(), guidCompany);
                    var request = actionContext.Request;
                    actionContext.Response = new System.Net.Http.HttpResponseMessage(HttpStatusCode.Forbidden);
                    return;
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Error en el middleware: {ex}");
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Error en la Solicitud HTTP desde el MonitorTracker Cliente"
                };
                return;
            }

            base.OnActionExecuting(actionContext);
        }

        private void NotificarUsuarioNoRegistrado(string Usuario, Guid IdCompany)
        {
            using (var context = new QueueContext())  
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {

                    try
                    {
                        bool existeUsuario = context.Agent_NotificacionUsuarios.Any(u => u.Usuario == Usuario &&
                            u.IdCompany == IdCompany &&
                            u.Descripcion == _NOTIFICACION_USUARIO_NO_REGISTRADO
                        );

                        if (!existeUsuario)
                        {
                            var nuevoRegistro = new Agent_NotificacionUsuario
                            {
                                IdNotificacion = Guid.NewGuid(),
                                IdCompany = IdCompany,
                                Usuario = Usuario.ToUpper(),
                                Descripcion = _NOTIFICACION_USUARIO_NO_REGISTRADO,
                                status = true,
                                Date = DateTime.Now,
                            };

                            context.Agent_NotificacionUsuarios.Add(nuevoRegistro);
                            context.SaveChanges();

                            dbContextTransaction.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                    }
                }
                
            }
        }

    }
}