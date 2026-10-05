using Newtonsoft.Json;
using Queue.ViewModels;
using System.Collections.Generic;
using System.Net;

namespace Queue.Models
{
    public class Response
    {
        public HttpStatusCode status { get; set; }
        public string response_code { get; set; }
        public string message { get; set; }
        public int cantidad { get; set; }
        public int pagina { get; set; }
        public string rute { get; set; }

        public object data = new object();
        public HoraryModel HoraryModel = new HoraryModel();
        public List<object> error = new List<object>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public USBPortsModel UsbManagementModel = new USBPortsModel();
    }
}