using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Queue.ViewModels
{
    public class ActivitySumViewModel
    {
        public string User { get; set; }
        public string program { get; set; }
        public int quantity { get; set; }
        public double time { get; set; }
        public Guid workArea { get; set; }
        public bool licensed { get; set; }
        public string codigoLicensed { get; set; }
        public string Url { get; set; }
    }
}