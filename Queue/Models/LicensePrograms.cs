using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Queue.Models
{
    [Table("LicensePrograms")]
    public class LicensePrograms
    {
        [Key]
        public int Id { get; set; }
        public Guid idprogramclasification { get; set; }
        public bool isLicensed { get; set; }
        public string licenseNumber { get; set; }
        public DateTime creationDate { get; set; }
        public DateTime? modifyDate { get; set; }

        public virtual Agent_ProgramClasification program { get; set; }
    }
}