using Newtonsoft.Json;
using Queue.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Queue.Models
{
    public class USBPortsModel
    {
        [JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("idempresa")]
        public string IdEmpresa { get; set; }

        public bool DenyAllAccessRemovableDevices { get; set; }

        public bool DenyAllDisableEnableDisk { get; set; }

        public List<USBPortsViewModel> USBPortsViewModel = new List<USBPortsViewModel>();
    } 
}