using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Queue.Models
{
    public class HoraryModel
    {
        public string token { get; set; }
        public string user { get; set; }
        public string idempresa { get; set; }
        public List<HoraryDetail> Agent_GroupHoraryDetail = new List<HoraryDetail>();
    }
}