using System;
using System.Threading;
using DependencyInjection;
using JetBrains.Annotations;
using Sidhe.Utilities.Interfaces;

namespace Sidhe.Utilities
{
    public class QueueWorker<TModel, TDispatcher> : Worker<CancellationToken>
        where TDispatcher : IDispatcher<TModel>
    {
        // ReSharper disable once StaticMemberInGenericType
        [NotNull] private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();


        private readonly TDispatcher _dispatcher;
        private readonly IBlockingQueue<TModel> _queue;
        

        public QueueWorker([NotNull] IFactory factory, [NotNull] TDispatcher dispatcher, [NotNull] IBlockingQueue<TModel> queue)
            : base(factory)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        }


        public virtual void OnDispatched([NotNull] TModel model, Exception exception = null)
        {
            if (exception != null)
            {
                Logger.Error(exception, $"Exception on dispatch of {typeof(TModel).Name}");
            }
        }


        protected override void Work(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var request = _queue.Dequeue();

                if (request == null)
                {
                    Logger.Warn($"Null read from non-blocking {typeof(TModel).Name} queue.");
                }
                else
                {
                    try
                    {
                        _dispatcher.Dispatch(request);
                        OnDispatched(request);
                    }
                    catch (Exception exception) when (!(exception is ThreadAbortException))
                    {
                        OnDispatched(request, exception);
                    }
                }
            }
        }
    }
}
