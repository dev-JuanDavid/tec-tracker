using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Queue.Models
{
    public class DashBoardStats
    {
        public Resume resume = new Resume();
        public List<GraphData> graph = new List<GraphData>();
        public List<BasicStatsDashboard> DataPerUser = new List<BasicStatsDashboard>();

        public DateTime DateFrom { get; set; } = DateTime.Today;
        public DateTime DateTo { get; set; } = DateTime.Today;
        public string ddlUsers { get; set; }
        public Guid idgroup { get; set; }
    }
}