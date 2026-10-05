using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Queue.ViewModels
{
    public class FileTransferViewModel
    {
        [JsonIgnore]
        public BsonBinaryData _id { get; set; } = new BsonBinaryData(Guid.NewGuid(), GuidRepresentation.Standard);

        [JsonProperty("IdEmpresa", NullValueHandling = NullValueHandling.Ignore)]
        public string IdEmpresa { get; set; }

        [JsonProperty("MachineName", NullValueHandling = NullValueHandling.Ignore)]
        public string MachineName { get; set; }

        [JsonProperty("UserName", NullValueHandling = NullValueHandling.Ignore)]
        public string UserName { get; set; }

        [JsonProperty("Date", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? Date { get; set; }

        [JsonProperty("Process ID")]
        public string ProcessID { get; set; }

        [JsonProperty("Process Name")]
        public string ProcessName { get; set; }

        [JsonProperty("Last Write Time")]
        public string LastWriteTime { get; set; }

        public string CreationTime { get; set; }

        [JsonProperty("Process Path")]
        public string ProcessPath { get; set; }

        [JsonProperty("Process Services")]
        public string ProcessServices { get; set; }

        [JsonProperty("FileName")]
        public string FileName { get; set; }
        public string Disk { get; set; }
        public string DiskType { get; set; }
        public string TypeOfEvent { get; set; }
        public string Path { get; set; }
        public bool IsDirectory { get; set; }
        public bool IsFile { get; set; }
        public string DayOfWeek { get; set; }
        public TimeSpan TimeOfDay { get; set; }
        public Guid LocalId { get; set; }

        [JsonProperty("FechaInsercion", NullValueHandling = NullValueHandling.Ignore)]
        public string FechaInsercion { get; set; }

        public string DataTransmission => $"ProcessServices: {ProcessServices} ";
    }
}