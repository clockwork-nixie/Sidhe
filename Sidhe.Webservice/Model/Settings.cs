using Sidhe.ApplicationServer.Interfaces;

namespace Sidhe.Webservice.Model
{
    public class Settings : IApplicationSettings
    {
        public int ClientReceiveBytes { get; set; }
        public int ClientSendBytes { get; set; }
        public bool IsConsoleEnabled { get; set; }
        public bool IsHostApplication { get; set; }
        public bool IsHostLogin { get; set; }
        public ushort LocalSessionPartitionCount { get; set; }
        public ushort WorldQueueCount { get; set; }
        public ushort WorldWorkersPerQueue { get; set; }
    }
}
