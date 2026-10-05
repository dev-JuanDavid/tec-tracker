using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Queue.Models
{
    [Table("Printer")]
    public class Printer
    {
        [Key]
        public int PrinterID { get; set; }

        public string Name { get; set; }

        public string code { get; set; }

        [Required]
        public bool Status { get; set; }

        public int? location_LocationID { get; set; }
    }
}