using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Queue.Models
{
    [Table("LogCodes")]
    public class LogCodes
    {
        [Key]
        public int LogCode { get; set; }

        [Required]
        [StringLength(300)]
        public string LogDescription { get; set; }
    }
}