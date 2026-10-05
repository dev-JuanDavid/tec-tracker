using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Queue.Models
{

    [Table("Agent_EmployeeUsb")]
    public class Agent_EmployeeUsb
    {
        [Key]
        public Guid EmployeeUsbId { get; set; }
        [ForeignKey("Agent_Employee")]
        public Guid idEmployee { get; set; }
        public string PortId { get; set; }
        public string Description { get; set; }
        public string Type { get; set; } = null;
        public bool Status { get; set; }
        public DateTime Date { get; set; }
       
        public virtual Agent_Employee Agent_Employee { get; set; }

    }

}