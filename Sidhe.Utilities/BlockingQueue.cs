using System;
using System.Collections.Concurrent;
using System.Threading;
using JetBrains.Annotations;
using Sidhe.Utilities.Interfaces;

namespace Sidhe.Utilities
{
    public class BlockingQueue<TDatum> : IBlockingQueue<TDatum>
        where TDatum : class
    {
        private CancellationToken _cancellationToken;
        [NotNull] private readonly ConcurrentQueue<TDatum> _queue = new ConcurrentQueue<TDatum>();
        [NotNull] private readonly ManualResetEventSlim _queueContainsItems = new ManualResetEventSlim(false);
        private SemaphoreSlim _queueHasEmptySlot;
        private bool _isConfigured;


        public IBlockingQueue<TDatum> Configure(CancellationToken? cancellationToken = null, int? approximateCapacityCap = null)
        {
            if (_isConfigured)
            {
                throw new InvalidOperationException("Queue is already started.");
            }

            if (approximateCapacityCap.HasValue)
            {
                _queueHasEmptySlot = new SemaphoreSlim(approximateCapacityCap.Value);
            }

            _cancellationToken = cancellationToken ?? CancellationToken.None;
            _isConfigured = true;

            return this;
        }


        public TDatum Dequeue(bool isBlockOnEmpty = true)
        {
            if (!_isConfigured)
            {
                throw new InvalidOperationException("Queue must be started before use.");
            }

            // The C# lock() is quite inefficient so bypass it if we can.
            // It does mean the the manual-reset-event we use to identify whether
            // there is data waiting is not totally accurate but it is safe and
            // we get good throughput without spin-waiting.
            if (!_queue.TryDequeue(out var result) && isBlockOnEmpty)
            {
                while (true)
                {
                    _queueContainsItems.Reset();

                    if (_queue.TryDequeue(out result))
                    {
                        if (!_queue.IsEmpty)
                        {
                            _queueContainsItems.Set();
                        }
                        break;
                    }
                    _queueContainsItems.Wait(_cancellationToken);
                }
            }

            if (result != null)
            {
                _queueHasEmptySlot?.Release();
            }

            return result;
        }


        public void Enqueue([NotNull] TDatum item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (!_isConfigured)
            {
                throw new InvalidOperationException("Queue must be started before use.");
            }

            _queueHasEmptySlot?.Wait(_cancellationToken);
            _queue.Enqueue(item);
            _queueContainsItems.Set();
        }

        
        public string Name { get; set; }
    }
}
