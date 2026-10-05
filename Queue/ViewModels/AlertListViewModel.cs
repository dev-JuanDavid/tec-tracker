using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Queue.ViewModels
{
    public class AlertListViewModel
    {
        public Guid id { get; set; }
        public string alert { get; set; }
        public string mail { get; set; }
    }
}