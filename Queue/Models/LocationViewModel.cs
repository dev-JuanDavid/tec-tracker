using Newtonsoft.Json;
using Queue.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Queue.Models
{
    public class LocationViewModel
    {
        //[JsonProperty("IdEmpresa")]
        //public string IdEmpresa { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("country_name")]
        public string CountryName { get; set; }

        [JsonProperty("region_name")]
        public string RegionName { get; set; }

        [JsonProperty("city_name")]
        public string CityName { get; set; }

        [JsonProperty("latitude")]
        public decimal Latitude { get; set; }

        [JsonProperty("longitude")]
        public decimal Longitude { get; set; }

        [JsonProperty("zip_code")]
        public string ZipCode { get; set; }

        [JsonProperty("time_zone")]
        public string TimeZone { get; set; }

        [JsonProperty("Mobile_brand")]
        public string MobileBrand { get; set; }

        [JsonProperty("Elevation")]
        public string Elevation { get; set; }
    } 
}