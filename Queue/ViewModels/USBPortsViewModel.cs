using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Queue.ViewModels
{
    public class USBPortsViewModel
    {
        [JsonIgnore]
        public BsonBinaryData _id { get; set; } = new BsonBinaryData(Guid.NewGuid(), GuidRepresentation.Standard);

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string User { get; set; }       
        public string DeviceID { get; set; }
        public string PnpDeviceID { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public bool Deactivate { get; set; }
        public string Type { get; set; } = null;

    }
}