using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Queue.Models
{
    [Table("Agent_EmployeesGroups")]
    public class Agent_EmployeesGroups
    {
        [Key]
        public Guid idemployeesGroup { get; set; }

        [DisplayName("Nombre")]
        [Required]
        public string Nombre { get; set; }

        public Agent_Empresa Agent_Empresa { get; set; }

        [NotMapped]
        public Guid idemployee { get; set; }


        [NotMapped]
        public List<Agent_Employee> Agent_EmployeeGroupsEmployee_list = new List<Agent_Employee>();

        public ICollection<Agent_ClasificationGroup> ClasificationGroups { get; set; } = new List<Agent_ClasificationGroup>();



    }
}