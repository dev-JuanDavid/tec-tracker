using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Queue.Models
{
    public class BasicStatsModel
    {
        public List<string> labels = new List<string>();
        public List<double?> data = new List<double?>();
        public List<string> DateTime = new List<string>();
    }
    public class BasicStatsDate
    {

        public List<string> DateTime = new List<string>();
    }

    public class BasicUserModel
    {
        public List<string> User = new List<string>();
        public List<string> Application = new List<string>();
        public List<double> Time = new List<double>();
        public List<DateTime> DateTime = new List<DateTime>();
    }

    public class ArrayUsersModel
    {
        public string User;
        public List<string> Application = new List<string>();
        public List<double> Time = new List<double>();
        public List<string> Date = new List<string>();
        //public List<ApplicationModel> Data = new List<ApplicationModel>();
    }
    public class ArrayUsersModelGantt
    {
        public string User;
        public List<string> Application = new List<string>();
        public List<double> Time = new List<double>();
        public List<string> Date = new List<string>();
        public List<string> AppImpro = new List<string>();
        public List<string> TimeImpro = new List<string>();
        //public List<ApplicationModel> Data = new List<ApplicationModel>();
    }

    public class ApplicationModel
    {
        public string Application;
        public double Time;
    }

    public class BasicStatsDashboard
    {
        public string User { get; set; }
        public string Application { get; set; }
        public int Clasification { get; set; }
        public double? Time { get; set; }
        public string Date { get; set; }
        public string URL { get; internal set; }
        public DateTime Date_ { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public double ProductiveTime { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public double ImproductiveTime { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public double NeutralTime { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public double UnclasifyTime { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public double TotalHours
        {
            get
            {
                return ProductiveTime + ImproductiveTime + NeutralTime + UnclasifyTime;
            }
        }
    }

    public class Resume
    {
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal ProductiveTime { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal ImproductiveTime { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal NeutralTime { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal UnclasifyTime { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal Total
        {
            get
            {
                return ProductiveTime + ImproductiveTime + NeutralTime + UnclasifyTime;
            }
        }
    }

    public class AppResume
    {
        [DisplayName("Aplicación")]
        public string appname { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal Time { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal PercentajeUsed { get; set; }
    }

    public class UsersReportGanttModel
    {
        public static string GetNameTimes(double Seconds)
        {
            TimeSpan timeSpan = new TimeSpan(0, 0, Convert.ToInt32(Math.Round(Seconds)));

            double _Seconds = timeSpan.Seconds;
            double _Minutes = timeSpan.Minutes;
            double _Hours = timeSpan.Hours;

            //double _Seconds = Seconds;
            //double _Minutes = _Seconds / 60;
            //double _Hours = _Minutes / 60;

            //string diff_show = Math.Round(_Seconds) + " segundos";
            //if (_Seconds >= 60)
            //{
            //    var _SecondsDiff = Math.Round((_Minutes * 60) - _Seconds);
            //    diff_show = Math.Round(_Minutes) + " minuto(s) " + (_SecondsDiff > 0 ? _SecondsDiff + " segundos" : string.Empty);
            //    if (_Minutes >= 60)
            //    {
            //        var _MinutesDiff = Math.Round((_Hours * 60) - _Minutes);
            //        diff_show = Math.Round(_Hours) + " hora(s) " + (_MinutesDiff > 0 ? _MinutesDiff + " minuto(s)" : string.Empty);
            //    }
            //}

            string diff_show = _Seconds + " segundos";
            if (_Hours > 0)
            {
                diff_show = _Hours + " hora(s) " + (_Minutes > 0 ? _Minutes + " minuto(s) " + (_Seconds > 0 ? _Seconds + " segundos" : string.Empty) : string.Empty);
            }
            else
            {
                if (_Minutes > 0)
                {
                    diff_show = _Minutes + " minuto(s) " + (_Seconds > 0 ? _Seconds + " segundos" : string.Empty);
                }
            }

            return diff_show;


        }

        public static string GetNameTimesApps(string Application, double Seconds, double SecondsIna)
        {
            return "<b>" + Application + ":</b> " + GetNameTimes(Seconds) + (SecondsIna > 0 ? " [Inactivo: " + GetNameTimes(SecondsIna) + " ]" : string.Empty);
        }

        private readonly string[] ColorClassification = { "#CEBAB6", "#7E72FA", "#FF0000", "#96989A" };
        public string UserName { get; set; }
        public List<string> GroupApps { get { return ReportSytems.Select(x => x.Application).Distinct().ToList(); } }
        public List<ReportTime> ReportSytems { get; set; } = new List<ReportTime>();
        public string URL { get; internal set; }
        public class ReportTime
        {
            public string Application { get; set; }
            public string ApplicationT
            {
                get
                {
                    List<string> AppsShow = new List<string>();
                    var grouping = GroupApplication.GroupBy(x => x.AppImproName).OrderBy(x => x.Key);
                    foreach (var item in grouping)
                    {
                        List<string> _Apps = new List<string>();
                        var AppsImproClassify = GroupApplication.Where(x => x.AppImproName == item.Key);
                        foreach (var itemApps in AppsImproClassify.Select(x => x.Application).Distinct())
                        {
                            _Apps.Add(GetNameTimesApps(itemApps, AppsImproClassify.Where(x => x.Application == itemApps).Sum(x => x.Activity), AppsImproClassify.Where(x => x.Application == itemApps).Sum(x => x.InActivity)));
                        }

                        AppsShow.Add("<u>" + item.Key + "</u> </br>" + string.Join("</br>", _Apps));
                    }

                    return string.Join("</br>", AppsShow);
                }
            }
            public string MessageDuration => GetNameTimes(TotalActivity);
            //public DateTime FocusTime { get; set; }



            public DateTime FocusTime { get; set; }


            public DateTime FocusTimeEnd => FocusTime.AddSeconds(TotalActivity);
            public string FocusTimeT => FocusTime.ToString("yyyy-MM-dd HH:mm:ss");

            public double Activity { get; set; }
            public double InActivity { get; set; }
            public double TotalActivity { get { return Activity + InActivity; } }
            public string AppImproName { get; set; }
            public int AppsImproClassify { get; set; }
            public string DateEnd => FocusTimeEnd.ToString("yyyy-MM-dd HH:mm:ss");

            public List<ReportTimeGroupApps> GroupApplication { get; set; } = new List<ReportTimeGroupApps>();
            public int GetAppsImproClassifyMoreUse
            {
                get
                {
                    var groupAppsImproClassify = GroupApplication.GroupBy(x => x.AppsImproClassify).Select(x => new { x1 = x.Key, x2 = x.Sum(xs => xs.Activity) }).OrderByDescending(xss => xss.x2);
                    if (groupAppsImproClassify.Any()) { return groupAppsImproClassify.First().x1; }
                    else { return 0; }
                }
            }

            public string URL { get; internal set; }
        }

        public class ReportTimeGroupApps
        {
            public int AppsImproClassify { get; set; }
            public string AppImproName { get; set; }
            public string Application { get; set; }
            public double Activity { get; set; }
            public double InActivity { get; set; }
        }

    }

    public class SoftwareReport
    {
        public string program { get; set; }
        public int quantity { get; set; }
        public string agrupation { get; set; }
        public DateTime instalationday { get; set; }
        public DateTime uninstalday { get; set; }

        public Guid idgroup { get; set; }
        public string user { get; set; }
    }

    public class HardwareReport
    {
        public string type { get; set; }
        public string hardware { get; set; }
        public int quantity { get; set; }
        public string agrupation { get; set; }
        public DateTime instalationday { get; set; }
        public DateTime uninstalday { get; set; }

        public Guid idgroup { get; set; }
        public string user { get; set; }
    }


    public class ProgramsReport
    {
        public string program { get; set; }
        public int quantity { get; set; }
        public string agrupation { get; set; }
        public DateTime instalationday { get; set; }
        public DateTime uninstalday { get; set; }
        public bool isLicensed { get; set; }
        public string licenseNumber { get; set; }

        public Guid workArea { get; set; }
        public string user { get; set; }
    }
    public class LicensedReport
    {
        public string program { get; set; }
        public bool isLicensed { get; set; }
        public string licenseNumber { get; set; }

        public int workArea { get; set; }
        public string user { get; set; }
    }

    public class NotificacionUsuarioReport
    {
        public string Usuario { get; set; }
        public string Descripcion { get; set; }

        public string Date { get; set; }

    }
}