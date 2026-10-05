using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Queue.ViewModels
{
    public class MostSearchedPagesViewModel
    {
        public string user { get; set; }
        public Guid IdEmployeesGroup { get; set; }
        public string idEmployee { get; set; }
        public string Url { get; set; }
        public SelectList WorkAreasList { get; set; }
        public SelectList UsersList { get; set; }
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        public List<ActivitySumViewModel> activities { get; set; } = new List<ActivitySumViewModel>();
    }
}
