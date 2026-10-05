using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Queue.Models
{
    [Table("WorkArea")]
    public class WorkArea
    {
        [Key]
        public int IdWorkArea { get; set; }

        //public Agent_Empresa Agent_Empresa { get; set; }
        public Guid IdCompany { get; set; }

        [DisplayName("WorkAreaName")]
        [Required]
        public string WorkAreaName { get; set; }

        //[NotMapped]
        //public Guid idemployee { get; set; }

        //[NotMapped]
        //public List<Agent_Employee> Agent_EmployeeAreaWork_list = new List<Agent_Employee>();
    }
}