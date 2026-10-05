using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace Queue.Models
{
    public class Functionality
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid IdFunctionality { get; set; }
        [DisplayName("Functionality Name")]
        public string Name { get; set; }
        [DisplayName("State")]
        public bool  Active { get; set; }

        [NotMapped]
        public Guid IdCompany { get; set; }
 
        public ICollection<CompanyFunctionality> CompanyFunctionalities { get; set; }
    }
}