using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Queue.Models
{
    [Table("Agent_Employee")]
    public class Agent_Employee
    {
        [Key]
        public Guid idEmployee { get; set; }

        public Guid IdCompany { get; set; }

        [DisplayName("Name")]
        [Required]
        public string Nombre { get; set; }

        [DisplayName("Address")]
        [Required]
        public string Direccion { get; set; }

        [DisplayName("Phone")]
        [Required]
        public string Telefono { get; set; }

        [Required]
        public string Email { get; set; }

        [DisplayName("Identification")]
        [Required]
        public string Identificacion { get; set; }

        [DisplayName("Username")]
        [Required]
        public string Usuario { get; set; }

        [Required]
        public Guid Cargo { get; set; }

        [DisplayName("Status")]
        public bool status { get; set; } = true;
        public string string_status { get; set; }


        //location
        public string Ip { get; set; }
        public string CodigoPais { get; set; }
        public string Pais { get; set; }
        public string Region { get; set; }
        public string Ciudad { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }

        [NotMapped]
        [DisplayName("Posible Error")]
        public string posibleerror { get; set; }

        [NotMapped]
        [DisplayName("Work Area")]
        public int IdWorkArea { get; set; }

        [NotMapped]
        [DisplayName("Work Area")]
        public Guid IdEmployeesGroup { get; set; }

        [ForeignKey("Agent_GroupHorary")]
        [DisplayName("Horario")]
        public Guid? Id_GroupHorary { get; set; }
        public Agent_GroupHorary Agent_GroupHorary { get; set; }

        [DisplayName("Departamento")]
        public Agent_CompanyDepartment Agent_CompanyDepartment { get; set; }


        //[ForeignKey("Agent_EmpoyeePorts")]
        [DisplayName("Puertos USB")]
        public List<Agent_EmployeeUsb> Agent_EmployeeUsb { get; set; }

        public ICollection<WorkAreaEmployee> WorkAreaEmployees { get; set; }

    }

}