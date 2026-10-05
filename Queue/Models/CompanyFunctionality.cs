using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Queue.Models
{
    public class CompanyFunctionality
    {
        public Guid IdFunctionality { get; set; }
        public Guid IdCompany { get; set; }

        public Functionality Functionality { get; set; }
        public Agent_Empresa AgentEmpresa { get; set; }


    }
}