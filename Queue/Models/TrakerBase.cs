using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Queue.Models
{
    public class TrakerBase
    {
        [Newtonsoft.Json.JsonIgnore]
        public BsonBinaryData _id { get; set; } = new BsonBinaryData(Guid.NewGuid(), GuidRepresentation.Standard);
        public string Application { get; set; }
        public string Title { get; set; }
        public Nullable<double> Time { get; set; }
        public Nullable<double> Inactivity { get; set; }
        public string Ip { get; set; }
        public string Pc { get; set; }
        public string UserName { get; set; }
        public double Activity { get; set; }
        public string Url { get; set; }

        [BsonElement]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]        
        public DateTime Date { get; set; }

        [BsonElement]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime FocusTime { get; set; }

        public string UploadFrecuency { get; set; }
        public string Frecuency { get; set; }
        public string prevvalue { get; set; }
        public string newvalue { get; set; }

        public string IdEmpresa { get; set; }
    }
}