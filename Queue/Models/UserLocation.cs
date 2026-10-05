using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Queue.Models
{
    [Table("UserLocation")]
    public class UserLocation
    {
        [Key]
        public Guid Id { get; set; }

        public Guid IdEmployee { get; set; }
        public Guid IdCompany { get; set; }
        public string Ip { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string RegionName { get; set; }
        public string CityName { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string ZipCode { get; set; }
        public string TimeZone { get; set; }
        public string MobileBrand { get; set; }
        public string Elevation { get; set; }
        public DateTime Fecha { get; set; }

    }

}