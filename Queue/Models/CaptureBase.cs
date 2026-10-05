using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Web;

namespace Queue.Models
{
    public class CaptureBase
    {
        [Newtonsoft.Json.JsonIgnore]
        public BsonBinaryData _id { get; set; } = new BsonBinaryData(Guid.NewGuid(), GuidRepresentation.Standard);
        public string idrecord { get; set; } = Guid.NewGuid().ToString();
        public string Ip { get; set; }
        public string Pc { get; set; }
        public string UserName { get; set; }
        public string IdCompany { get; set; }
        public byte[] Image { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow.AddHours(-5);
        public int Hour { get; set; } = DateTime.UtcNow.AddHours(-5).Hour;
    }
}