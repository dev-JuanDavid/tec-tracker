using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Queue.Models
{
    public class LocationHistory
    {
        public string Name { get; set; }
        public string User { get; set; }
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
        public DateTime Date { get; set; }
    }
}