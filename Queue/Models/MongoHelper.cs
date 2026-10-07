using MongoDB.Driver;
using Queue.ViewModels;
using System;
using System.Web.Configuration;

namespace Queue.Models
{
    public class MongoHelper
    {
        public static IMongoClient client { get; set; }
        public static IMongoDatabase database { get; set; }

        public static string MongoConnection = WebConfigurationManager.AppSettings["MongoConnectionString"];
        public static string MongoDatabase = WebConfigurationManager.AppSettings["MongoDatabase"];

        public static IMongoCollection<TrakerBase> TrakerBase { get; set; }
        public static IMongoCollection<InstalledHardwareViewModel> HardWareList { get; set; }
        public static IMongoCollection<InstalledProgramsViewModel> SoftWareList { get; set; }
        public static IMongoCollection<CaptureBase> UserCapture { get; set; }
        public static IMongoCollection<LogsViewModel> Logs { get; set; }

        //SCI Software
        public static IMongoCollection<FileTransferViewModel> FileTransfer { get; set; }
        public static IMongoCollection<USBPortsViewModel> USBPorts { get; set; }

        internal static void ConnectToMongoService()
        {
            try
            {
                client = new MongoClient(MongoConnection);
                database = client.GetDatabase(MongoDatabase);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
