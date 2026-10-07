using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Queue.Models
{
    [Table("Agent_Empresa")]
    public class Agent_Empresa
    {
        [Key]
        public Guid IdCompany { get; set; }
        [DisplayName("Nombre")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Nombre { get; set; }
        [DisplayName("Dirección")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Direccion { get; set; }
        [DisplayName("Teléfono")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Telefono { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DisplayName("Correo electrónico")]
        [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DisplayName("Identificación tributaria")]
        public string Rut { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DisplayName("Clave de la empresa")]
        public string Key { get; set; }
        public decimal Id_EmpresaBPM { get; set; }
        [DisplayName("Estado")]
        public bool status { get; set; } = true;
        public string string_status { get; set; }
        public ICollection<CompanyFunctionality> CompanyFunctionalities { get; set; }
    }
}