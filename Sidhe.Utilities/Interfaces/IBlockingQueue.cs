using System.Threading;
using JetBrains.Annotations;

namespace Sidhe.Utilities.Interfaces
{
    public interface IBlockingQueue<TDatum>
    {
        [NotNull] IBlockingQueue<TDatum> Configure(CancellationToken? cancellationToken = null, int? approximateCapacity = null);
        TDatum Dequeue(bool isBlockOnEmpty = true);
        void Enqueue(TDatum item);
        string Name { get; }
    }
}