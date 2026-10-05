using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Queue.Models
{
    [Table("Location")]
    public class Location
    {
        [Key]
        public int LocationID { get; set; }

        public string Name { get; set; }

        public string ClosingTime { get; set; }
    }
}