using Queue.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Queue.Models
{
    public class Agent_EmployeeUsbConfig
    {
        public Guid idEmployee { get; set; }

        public List<Agent_EmployeeUsb> Agent_EmployeeUsb { get; set; }

        public ICollection<WorkAreaEmployee> WorkAreaEmployees { get; set; }

        public bool IsDisabled { get; set; }

        public bool IsDisk { get; set; }
    }
}