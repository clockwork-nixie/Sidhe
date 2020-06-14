using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DependencyInjection;
using JetBrains.Annotations;
using Sidhe.Utilities.Interfaces;

namespace Sidhe.Utilities
{
    public class QueueWorkerSet<TDatum, TWorker, TQueue> : Dependency, IQueueWorkerSet<TDatum, TWorker, TQueue>
        where TDatum : class
        where TWorker : IWorker<CancellationToken>
        where TQueue : class, IBlockingQueue<TDatum>
    {
        [NotNull] private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        [NotNull] private readonly object _lock = new  object();

        private bool _isStarted;
        private Func<TDatum, int> _hashFunction;
        private IBlockingQueue<TDatum>[] _queues;
        private IWorker[] _workers;
        private WaitHandle[] _waitHandles;


        public QueueWorkerSet([NotNull] IFactory factory) : base(factory) { }


        public void Dispose()
        {
            lock (_lock)
            {
                if (_workers != null)
                {
                    foreach (var worker in _workers)
                    {
                        worker?.Dispose();
                    }
                }
            }
        }

        public void Configure(int queueCount, int workersPerQueue, CancellationToken cancellationToken,
            Func<TQueue, TWorker> workerFactory, Func<TDatum, int> hashFunction = null)
        {
            if (queueCount <= 0)
            {
                throw new ArgumentException($"{nameof(queueCount)} must be positive but {queueCount} was passed.");
            }

            if (workersPerQueue <= 0)
            {
                throw new ArgumentException($"{nameof(workersPerQueue)} must be positive but {workersPerQueue} was passed.");
            }

            lock (_lock)
            {
                if (_queues != null)
                {
                    throw new InvalidOperationException("Worker-set already configured.");
                }
                
                Logger.Info($"Creating {queueCount} queue(s) with {workersPerQueue} worker(s) each.");

                var queues = new IBlockingQueue<TDatum>[queueCount];
                var workers = new List<IWorker>();
                var workerCount = 0;

                for (var queueIndex = 0; queueIndex < queues.Length; ++queueIndex)
                {
                    var queue = GetInstance<TQueue>();
                    
                    queue.Configure(cancellationToken);

                    for (var workerIndex = 0; workerIndex < workersPerQueue; ++workerIndex)
                    {
                        var worker = workerFactory(queue);

                        worker.Name = $"WorldWorker #{++workerCount}";
                        workers.Add(worker.Bind(cancellationToken));
                    }
                
                    queues[queueIndex] = queue;
                }

                _hashFunction = hashFunction;
                _queues = queues;
                _workers = workers.ToArray();
                _waitHandles = _workers.Select(w => w.FinishedHandle).ToArray();
            }
        }


        public void QueueRequest(TDatum request)
        {
            if (request != null)
            {
                lock (_lock)
                {
                    if (!_isStarted)
                    {
                        throw new InvalidOperationException("Queue-set is not started.");
                    }

                    var index = Math.Abs(_hashFunction?.Invoke(request) ?? request.GetHashCode()) % _queues.Length;

                    _queues[index].Enqueue(request);
                }
            }
        }


        public void Start()
        {
            lock (_lock)
            {
                if (_isStarted)
                {
                    throw new InvalidOperationException("Queue-set is already started.");
                }

                foreach (var worker in _workers)
                {
                    worker.Start();
                }

                _isStarted = true;
            }
        }


        public IEnumerable<WaitHandle> WaitHandles
        {
            get
            {
                if (_waitHandles == null)
                {
                    throw new InvalidOperationException("Worker-set not yet configured.");
                }

                return _waitHandles;
            }
        }
    }
}
