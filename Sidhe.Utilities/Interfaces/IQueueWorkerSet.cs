using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Sidhe.Utilities.Interfaces
{
    public interface IQueueWorkerSet<TDatum, in TWorker, out TQueue> : IDisposable
        where TDatum : class
        where TWorker : IWorker<CancellationToken>
        where TQueue : class, IBlockingQueue<TDatum>
    {
        void Configure(int queueCount, int workersPerQueue, CancellationToken cancellationToken, 
            [NotNull] Func<TQueue, TWorker> workerFactory, Func<TDatum, int> hashFunction = null);

        void QueueRequest(TDatum request);
        void Start();
        [NotNull] IEnumerable<WaitHandle> WaitHandles { get; }
    }
}
