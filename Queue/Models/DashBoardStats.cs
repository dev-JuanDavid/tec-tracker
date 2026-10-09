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
        public List<DashboardApplicationUsage> ApplicationUsage { get; set; } = new List<DashboardApplicationUsage>();
        public List<DashboardActivityDay> ActivityPerDay { get; set; } = new List<DashboardActivityDay>();
        public double RegisteredSeconds { get; set; }
        public int ApplicationCount { get; set; }

        public DateTime DateFrom { get; set; } = DateTime.Today;
        public DateTime DateTo { get; set; } = DateTime.Today;
        public string ddlUsers { get; set; }
        public Guid idgroup { get; set; }
    }

    public class DashboardApplicationUsage
    {
        public string Application { get; set; }
        public double Seconds { get; set; }
    }

    public class DashboardActivityDay
    {
        public string Date { get; set; }
        public double ProductiveSeconds { get; set; }
        public double ImproductiveSeconds { get; set; }
        public double NeutralSeconds { get; set; }
        public double UnclassifiedSeconds { get; set; }
    }
}
