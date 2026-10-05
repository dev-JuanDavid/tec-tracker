using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Queue.Models
{
	[Table("Alertas")]
	public class Alertas
	{
		[Key]
		public Guid Id { get; set; }
		public string Alerta { get; set; }
        public int type { get; set; }
    }
}