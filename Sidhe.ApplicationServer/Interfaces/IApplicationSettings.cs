namespace Sidhe.ApplicationServer.Interfaces
{
    public interface IApplicationSettings
    {
        int ClientReceiveBytes { get; }
        int ClientSendBytes { get; }
        public bool IsConsoleEnabled { get; }
        public bool IsHostApplication { get; }
        public bool IsHostLogin { get; set; }
        ushort WorldQueueCount { get; }
        ushort WorldWorkersPerQueue { get; }
        ushort LocalSessionPartitionCount { get; set; }
    }
}
