using Newtonsoft.Json;
using Queue.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Queue.Models
{

    public class FileTransferModel
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("idempresa")]
        public string IdEmpresa { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("fileTransferViewModel")] 
        public List<FileTransferViewModel> FileTransferViewModel = new List<FileTransferViewModel>();
    } 
}