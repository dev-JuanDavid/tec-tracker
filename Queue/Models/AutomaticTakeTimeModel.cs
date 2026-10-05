using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Queue.Models
{
    public class AutomaticTakeTimeModel
    {
        public System.Guid Id { get; set; }
        public string Application { get; set; }
        public string Title { get; set; }
        public string newvalue { get; set; }
        public string Url { get; set; }
        public Nullable<double> Time { get; set; }
        public Nullable<double> Inactivity { get; set; }

        public string Ip { get; set; }
        public string Pc { get; set; }
        public string UserName { get; set; }
        public string User { get; set; }
        public string IdEmpresa { get; set; }

        public double Activity { get; set; }

        [BsonElement]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Date { get; set; }

        [BsonElement]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime FocusTime { get; set; }

        public string UploadFrecuency { get; set; }
        public string Frecuency { get; set; }
        public string prevvalue { get; set; }

        [Display(Name = "token")]
        [JsonProperty("token")]
        public string token { get; set; }
    }
}