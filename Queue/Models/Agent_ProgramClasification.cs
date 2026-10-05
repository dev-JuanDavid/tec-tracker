using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Queue.Models
{
    [Table("Agent_ProgramClasification")]
    public class Agent_ProgramClasification
    {
        [Key]
        public Guid idprogramclasification { get; set; }

        [DisplayName("Nombre")]
        [Required]
        public string name { get; set; }

        [DisplayName("Titulo")]
        public string title { get; set; }

        [DisplayName("Clasificación")]
        [Required]
        public int clasification { get; set; }

        public virtual Agent_Empresa Agent_Empresa { get; set; }

        [DisplayName("Estado de la licencia")]
        [NotMapped]
        public bool isLicensed { get; set; } = false;

        [DisplayName("Numero de licencia")]
        [NotMapped]
        public string licenseNumber { get; set; }

        [NotMapped]
        public DateTime creationDate { get; set; }

        [NotMapped]
        public DateTime? modifyDate { get; set; }

        [NotMapped]
        public List<AutomaticTakeTimeModel> AutomaticTakeTime = new List<AutomaticTakeTimeModel>();

        public List<Agent_ClasificationGroup> ClasificationGroups { get; set; } = new List<Agent_ClasificationGroup>();
    }
}