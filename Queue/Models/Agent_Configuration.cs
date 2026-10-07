using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Queue.Models
{
    [Table("Agent_Configuration")]
    public class Agent_Configuration
    {
        [Key]
        
        public Guid Id_Configuration { get; set; }
        [Required]
        [DisplayName("Período de inactividad")]
        [Range(0, int.MaxValue, ErrorMessage = "Ingresa un valor igual o mayor que cero.")]
        public int InactivityPeriod { get; set; } = 10;
        [Required]
        [DisplayName("Frecuencia de envío")]
        [Range(0, int.MaxValue, ErrorMessage = "Ingresa un valor igual o mayor que cero.")]
        public int UploadFrecuency { get; set; } = 10;
        [Required]
        [DisplayName("Frecuencia de capturas")]
        [Range(0, int.MaxValue, ErrorMessage = "Ingresa un valor igual o mayor que cero.")]
        public int CaptureFrecuency { get; set; } = 10;
        public Guid? IdCompany { get; set; }
        public virtual Agent_Empresa Agent_Empresa { get; set; }

        [Required]
        [DisplayName("Frecuencia de ubicación")]
        [Range(0, int.MaxValue, ErrorMessage = "Ingresa un valor igual o mayor que cero.")]
        public int LocationFrecuency { get; set; }

        [Required]
        [DisplayName("Fecha de creación")]
        public DateTime DateCreation { get; set; }
    }
}