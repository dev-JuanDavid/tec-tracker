using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Queue.ViewModels
{
    public class MostUsedAppsViewModel
    {
        //public int IdWorkArea { get; set; }
        public string user { get; set; }


        public Guid IdEmployeesGroup { get; set; }  
        public string idEmployee { get; set; }
        public SelectList WorkAreasList { get; set; }
        public SelectList UsersList { get; set; }

        public DateTime from { get; set; }
        public DateTime to { get; set; }
        public List<ActivitySumViewModel> activities = new List<ActivitySumViewModel>();
    }
}