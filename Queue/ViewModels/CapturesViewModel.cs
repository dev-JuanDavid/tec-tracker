using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Queue.ViewModels
{
    public class CapturesViewModel
    {
        public string _id { get; set; }
        public string idrecord { get; set; }
        public string Ip { get; set; }
        public string Pc { get; set; }
        public string UserName { get; set; }
        public string IdCompany { get; set; }
        public byte[] Image { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; } = DateTime.Now;

        public DateTime DateFrom { get; set; } = DateTime.Now;
        public DateTime DateTo { get; set; } = DateTime.Now;
        public String image { get; set; }

        public Guid idgroup { get; set; }
    }
}