using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static Queue.Utils.Enums;

namespace Queue.Models
{
    public class HoraryDetail
    {
        public EnumDays Day { get; set; }

        public DateTime HourFrom { get; set; }

        public DateTime HourUntil { get; set; }

        

        public TimeSpan Timefrom
        {
            get
            {
                return HourFrom.TimeOfDay;
            }
        }

        public TimeSpan Timeto
        {
            get
            {
                return HourUntil.TimeOfDay;
            }
        }       

        public EnumType Type { get; set; }

        public Guid Id_GroupHorary { get; set; }
    }
}