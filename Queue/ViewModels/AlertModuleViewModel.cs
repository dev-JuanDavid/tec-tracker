using Queue.Models;
using System.Collections.Generic;

namespace Queue.ViewModels
{
    public class AlertModuleViewModel
    {
        public string id { get; set; }
        public AlertAsociados AlertAsociados { get; set; }

        public List<AlertListViewModel> AlertListViewModel = new List<AlertListViewModel>();
    }
}