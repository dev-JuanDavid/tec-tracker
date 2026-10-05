using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Queue.Models
{
    [Table("Agent_NotificacionUsuario")]
    public class Agent_NotificacionUsuario
    {
        [Key]
        public Guid IdNotificacion { get; set; }
        public Guid IdCompany { get; set; }

        [DisplayName("Username")]
        [Required]
        public string Usuario { get; set; }

        [DisplayName("Descripcion")]
        [Required]
        public string Descripcion { get; set; }

        [DisplayName("Status")]
        public bool status { get; set; } = true;

        [Required]
        public DateTime Date { get; set; }
    }
}