using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Queue.ViewModels
{
    public class AutomaticTakeTimeModelViewModel
    {
        public System.Guid Id { get; set; }
        public string Application { get; set; }
        public string Title { get; set; }
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
        public string DateSpam { get; set; }
        public DateTime FocusTime { get; set; }                
        public string token { get; set; }

        public string StringDate { get; set; }
        public string StringFocusTime { get; set; }
    }
}