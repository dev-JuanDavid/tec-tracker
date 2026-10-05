using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Queue.Models
{
    [Table("Alerts")]
    public class Alerts
    {
        [Key]
        public Guid IdAlerts { get; set; }
        public int tipo { get; set; }
        public virtual Agent_Employee Agent_Employee { get; set; }
        public DateTime date { get; set; }
        public bool status { get; set; }
    }
}