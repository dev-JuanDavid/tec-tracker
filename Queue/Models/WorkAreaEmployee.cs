using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;

namespace Queue.Models
{
    [Table("WorkAreaEmployee")]
    public class WorkAreaEmployee
    {
        [Key]
        public int Id { get; set; }

        //public WorkArea WorkArea { get; set;}
        public Guid IdWorkArea { get; set; }
        //public Agent_Employee Agent_Employee { get; set; }
        public Guid idEmployee { get; set; }

        public virtual Agent_Employee employee { get; set; }

    }
}