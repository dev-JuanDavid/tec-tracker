using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Queue.ViewModels
{
    public class InstalledVsLicensedViewModel
    {
        public Guid IdEmployeesGroup { get; set; }
        public string idEmployee { get; set; }

        public DateTime from { get; set; }
        public DateTime to { get; set; }

        public string user { get; set; }

        public List<ProgramsLicensedActivityViewModel> activities { get; set; }
    }
}