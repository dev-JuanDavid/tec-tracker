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
    public class aPortsMachine
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        aUtilities autil = new aUtilities();
        ClaimsPrincipal claims = new ClaimsPrincipal();
        TokenValidationHandler tokenValidation = new TokenValidationHandler();

        //CREA LOS PUERTOS USB DISPONIBLES QUE TIENN LOS USUARIOS EN SUS MAQUINAS
        public object InventoryPorts(USBPortsModel model)
        {
            SaveDebugLog(model);
            Response response = new Response();
            int index = 1;
            Guid empresaId = Guid.Parse(model.IdEmpresa.ToString());
            try
            {
                claims = tokenValidation.getprincipal(Convert.ToString(model.Token));
                if (claims != null)
                {
                    List<USBPortsViewModel> ports = new List<USBPortsViewModel>();

                    if (model.USBPortsViewModel.Count() > 0)
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

                            foreach (var port in model.USBPortsViewModel)
                            {
                                var portsDb = ent.Agent_EmployeeUsb.Where(r => r.idEmployee == empoyee.idEmployee && r.PortId == port.DeviceID).FirstOrDefault();
                                if (portsDb == null)
                                {
                                    //Inserta nuevo puerto
                                    var newPort = new Agent_EmployeeUsb
                                    {
                                        EmployeeUsbId = Guid.NewGuid(),
                                        idEmployee = empoyee.idEmployee,
                                        Description = port.Name,
                                        PortId = port.DeviceID,
                                        Status = port.Deactivate,
                                        Type = port.Type,
                                        Date = DateTime.UtcNow.AddHours(-5)
                                    };
                                    ent.Agent_EmployeeUsb.Add(newPort);
                                    ent.SaveChanges();
                                }

                                index++;
                            }

                            var agent_EmployeeUsbPorts = ent.Agent_EmployeeUsb.Where(r => r.idEmployee == empoyee.idEmployee).ToList();

                            foreach (var item in agent_EmployeeUsbPorts)
                            {
                                var portDelete = model.USBPortsViewModel.Where(x => x.DeviceID == item.PortId).FirstOrDefault();
                                if (portDelete == null)
                                {
                                    var registro = ent.Agent_EmployeeUsb.Find(item.EmployeeUsbId);
                                    ent.Agent_EmployeeUsb.Remove(registro);
                                    ent.SaveChanges();
                                }
                            }

                            bool usbConfig = false;  // Por defecto
                            bool diskConfig = false; // Por defecto
                            //Pregunta si esta desactivado el indicador global
                            if (usbConfig)
                            {
                                // Obtener la lista de dispositivos con la descripción "almacenamiento"
                                var storageDevices = ent.Agent_EmployeeUsb
                                    .Where(usb => usb.idEmployee == empoyee.idEmployee
                                                  && usb.Description.Contains("almacenamiento"))
                                    .ToList();

                                // Actualizar el estado de los puertos USB de almacenamiento
                                foreach (var usb in storageDevices)
                                {
                                    usb.Status = usbConfig; // Desactivar o activar según el checkbox
                                    usb.Date = DateTime.UtcNow.AddHours(-5); // Actualizar la fecha
                                    ent.Entry(usb).State = EntityState.Modified;
                                }

                                ent.SaveChanges(); // Guardar los cambios
                            } else
                            {
                                var storageDevices = ent.Agent_EmployeeUsb
                                    .Where(usb => usb.idEmployee == empoyee.idEmployee && usb.Status == true
                                                  && usb.Description.Contains("almacenamiento"))
                                    .ToList();

                                // Actualizar el estado de los puertos USB de almacenamiento
                                foreach (var usb in storageDevices)
                                {
                                    usb.Status = false; // Desactivar o activar según el checkbox
                                    usb.Date = DateTime.UtcNow.AddHours(-5); // Actualizar la fecha
                                    ent.Entry(usb).State = EntityState.Modified;
                                }

                                ent.SaveChanges(); // Guardar los cambios

                            }
                                
                            


                            var employeeUsbPorts = ent.Agent_EmployeeUsb.Where(r => r.idEmployee == empoyee.idEmployee).ToList();
                            List<USBPortsViewModel> resultPorts = employeeUsbPorts.Select(x=> new USBPortsViewModel() {
                                User = model.User,
                                DeviceID = x.PortId,
                                PnpDeviceID = x.PortId,
                                Name = x.Description,
                                Deactivate = x.Status,
                                Status = x.Status == true ? "OK" : "Error"
                            }).OrderBy(x=> x.Name).ToList();


                            int denyAllAccessRemovableDevices = agent_EmployeeUsbPorts.Where(x=> x.Status == true).Count();
                            int denyAllDisableEnableDisk = agent_EmployeeUsbPorts.Where(x => x.Status == true).Count();
                            response.response_code = GenericErrors.SaveOk.ToString();
                            response.UsbManagementModel = new USBPortsModel
                            {
                                DenyAllAccessRemovableDevices = denyAllAccessRemovableDevices > 0 ? true : false,
                                DenyAllDisableEnableDisk = diskConfig ? true : false, // Nuevo contador para dispositivos deshabilitados
                                IdEmpresa = model.IdEmpresa,
                                User = model.User,
                                USBPortsViewModel = resultPorts
                            };

                            

                        }
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