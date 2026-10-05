using System;

namespace Queue.Models
{
    public class GeoLocationModel
    {
        public Guid EmployeeId { get; set; }
        public string Username { get; set; }
        public string Ip { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string RegionName { get; set; }
        public string CityName { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public DateTime? Date { get; set; }
    }
}