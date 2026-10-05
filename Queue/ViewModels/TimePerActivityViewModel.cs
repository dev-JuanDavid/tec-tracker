using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Queue.ViewModels
{
    public class TimePerActivityViewModel
    {
        public Guid idgruoup { get; set; }
        public string user { get; set; }
        public DateTime from { get; set; }
        public DateTime to { get; set; }

        public List<ActivitySumViewModel> activities = new List<ActivitySumViewModel>();
    }
}