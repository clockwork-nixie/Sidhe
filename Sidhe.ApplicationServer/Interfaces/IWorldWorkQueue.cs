using Sidhe.ApplicationServer.Model;
using Sidhe.Utilities.Interfaces;

namespace Sidhe.ApplicationServer.Interfaces
{
    public interface IWorldWorkQueue : IBlockingQueue<WorldWorkRequest> { }
}
