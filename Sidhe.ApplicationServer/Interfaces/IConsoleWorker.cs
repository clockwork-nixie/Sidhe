using System.Threading;
using Sidhe.ApplicationServer.Terminal;
using Sidhe.Utilities.Interfaces;

namespace Sidhe.ApplicationServer.Interfaces
{
    public interface IConsoleWorker : IWorker<CancellationToken>
    {
        public event ConsoleWorker.SessionRequestHandler OnSessionRequest;
    }
}