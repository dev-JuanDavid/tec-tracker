using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Queue.Models
{
    [Table("AlertAsociados")]
    public class AlertAsociados
    {
        [Key]
        public Guid Id { get; set; }
        public virtual Alertas Alertas { get; set; }
        public string Email { get; set; }
        public Guid IdCompany { get; set; }
        public virtual Agent_Empresa Agent_Empresa { get; set; }


    }
}