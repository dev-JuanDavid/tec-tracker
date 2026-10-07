using System;

namespace Queue.Models
{
    public class ParameterSystem
    {
        public Guid Id_Configuration { get; set; }
        [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue, ErrorMessage = "Ingresa un número entero igual o mayor que cero.")]
        public int InactivityPeriod { get; set; }
        [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue, ErrorMessage = "Ingresa un número entero igual o mayor que cero.")]
        public int UploadFrecuency { get; set; }
        [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue, ErrorMessage = "Ingresa un número entero igual o mayor que cero.")]
        public int CaptureFrecuency { get; set; }
        [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue, ErrorMessage = "Ingresa un número entero igual o mayor que cero.")]
        public int LocationFrecuency { get; set; }
        public Guid? IdCompany { get; set; }
        public string Company { get; set; }
        public DateTime DateCreation { get; set; }
    }
}
