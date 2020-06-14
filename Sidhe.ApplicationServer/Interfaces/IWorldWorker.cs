using System.Threading;
using Sidhe.Utilities.Interfaces;

namespace Sidhe.ApplicationServer.Interfaces
{
    public interface IWorldWorker : IWorker<CancellationToken> { }
}
