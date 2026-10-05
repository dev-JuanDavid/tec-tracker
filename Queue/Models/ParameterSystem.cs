using System;

namespace Queue.Models
{
    public class ParameterSystem
    {
        public Guid Id_Configuration { get; set; }
        public int InactivityPeriod { get; set; }
        public int UploadFrecuency { get; set; }
        public int CaptureFrecuency { get; set; }
        public int LocationFrecuency { get; set; }
        public Guid? IdCompany { get; set; }
        public string Company { get; set; }
        public DateTime DateCreation { get; set; }
    }
}