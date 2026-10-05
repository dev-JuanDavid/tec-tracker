

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Queue.Models
{
    public class Agent_ClasificationGroup
    {
         [Key]
        public Guid Id { get; set; }

        public Guid idprogramclasification { get; set; }

        public Agent_ProgramClasification Agent_ProgramClasifications { get; set; }
        public Guid idemployeesGroup { get; set; }

        public virtual Agent_EmployeesGroups Agent_EmployeesGroups { get; set; }

        [DisplayName("Clasificación")]
        public int clasification { get; set; }

    }
}