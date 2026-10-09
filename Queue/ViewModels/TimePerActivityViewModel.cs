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

    public class ActivityTrendViewModel
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public Guid GroupId { get; set; }
        public string User { get; set; }
        public bool HasQuery { get; set; }
        public DateTime PreviousFrom { get; set; }
        public DateTime PreviousTo { get; set; }
        public ActivityTrendPeriodSummary Current { get; set; } = new ActivityTrendPeriodSummary();
        public ActivityTrendPeriodSummary Previous { get; set; } = new ActivityTrendPeriodSummary();
        public List<ActivityTrendClassification> Classifications { get; set; } = new List<ActivityTrendClassification>();
        public List<ActivityTrendApplication> Applications { get; set; } = new List<ActivityTrendApplication>();
        public List<ActivityTrendEmployee> Employees { get; set; } = new List<ActivityTrendEmployee>();
        public List<ActivityTrendDay> Days { get; set; } = new List<ActivityTrendDay>();
    }

    public class ActivityTrendPeriodSummary
    {
        public double TotalSeconds { get; set; }
        public double ProductiveSeconds { get; set; }
        public double ImproductiveSeconds { get; set; }
        public double NeutralSeconds { get; set; }
        public double UnclassifiedSeconds { get; set; }
        public int DaysWithRecords { get; set; }
        public int DaysWithoutRecords { get; set; }
    }

    public class ActivityTrendClassification
    {
        public string Name { get; set; }
        public double CurrentSeconds { get; set; }
        public double PreviousSeconds { get; set; }
        public double DeltaSeconds { get; set; }
    }

    public class ActivityTrendApplication
    {
        public string Application { get; set; }
        public double CurrentSeconds { get; set; }
        public double PreviousSeconds { get; set; }
        public double DeltaSeconds { get; set; }
    }

    public class ActivityTrendEmployee
    {
        public string User { get; set; }
        public double CurrentSeconds { get; set; }
        public double PreviousSeconds { get; set; }
        public double DeltaSeconds { get; set; }
    }

    public class ActivityTrendDay
    {
        public DateTime CurrentDate { get; set; }
        public DateTime PreviousDate { get; set; }
        public double CurrentSeconds { get; set; }
        public double PreviousSeconds { get; set; }
        public bool CurrentHasRecords { get; set; }
        public bool PreviousHasRecords { get; set; }
        public double DeltaSeconds { get; set; }
    }
}
