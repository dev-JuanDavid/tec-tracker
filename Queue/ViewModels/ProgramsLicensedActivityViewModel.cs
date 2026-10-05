using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Queue.ViewModels
{
    public class ProgramsLicensedActivityViewModel
    {
        public string User { get; set; }
        public Guid WorkArea { get; set; }
        public string program { get; set; }
        public double time { get; set; }
        public bool isUsed { get; set; }
        public bool isInstalled { get; set; }
        public int numUser { get; set; }
        public bool isLicensed { get; set; }
        public string License { get; set; }
    }
}