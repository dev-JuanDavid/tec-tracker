using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Queue.Models
{
    [Table("License")]
    public class License
    {
        [Key]
        public Guid IdLicense { get; set; }

        [DisplayName("Fecha Limite Licencia")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime enddate { get; set; }

        public virtual Agent_Empresa Agent_Empresa { get; set; }

        [NotMapped]
        public Guid idempresa { get; set; }
    }
}