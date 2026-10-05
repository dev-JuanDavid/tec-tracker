using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Queue.ViewModels
{
    public class AlertsDataViewModel
    {
        public string idempresa { get; set; }
        public string application { get; set; }
        public int? clasification { get; set; }
        public double activity { get; set; }
        public double inactivity { get; set; }
        public string username { get; set; }
    }
}