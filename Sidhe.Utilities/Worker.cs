using System;
using System.Threading;
using DependencyInjection;
using JetBrains.Annotations;
using Sidhe.Utilities.Interfaces;

namespace Sidhe.Utilities
{
    public abstract class Worker<TState> : Dependency, IWorker<TState>
    {
        // ReSharper disable once StaticMemberInGenericType
        [NotNull] private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        [NotNull] private readonly ManualResetEventSlim _finished = new ManualResetEventSlim();
        [NotNull] private readonly Thread _thread;


        protected class WorkerBinding : IWorker
        {
            private readonly TState _state;
            [NotNull] private readonly IWorker<TState> _worker;

            public WorkerBinding([NotNull] IWorker<TState> worker, TState state)
            {
                _state = state;
                _worker = worker;
            }

            public void Dispose() => _worker.Dispose();
            public WaitHandle FinishedHandle => _worker.FinishedHandle;
            public string Name { get => _worker.Name; set => _worker.Name = value; }
            public virtual void Start() => _worker.Start(_state);
        }


        protected Worker([NotNull] IFactory factory) : base(factory) => _thread = new Thread(WorkerMain) {IsBackground = true};


        public virtual IWorker Bind(TState state) => new WorkerBinding(this, state);


        public void Dispose()
        {
            if (_thread.ThreadState != ThreadState.Unstarted)
            {
                try
                {
                    _thread.Join();
                }
                catch (Exception exception)
                {
                    Logger.Warn(exception,$"Worker thread {Name} failed to join during disposal.");
                }
            }

            _finished.Dispose();
        }


        public WaitHandle FinishedHandle => _finished.WaitHandle;


        public string Name
        {
            get => _thread.Name ?? GetType().Name;
            set => _thread.Name = value;
        }


        private void WorkerMain(object state)
        {
            try
            {
                Work((TState)state);
            }
            catch (OperationCanceledException)
            {
                Logger.Info($"Worker thread {Name} terminated by cancellation.");
            }
            catch (Exception exception) when (!(exception is ThreadAbortException))
            {
               Logger.Error(exception, $"Worker thread {Name} terminated unexpectedly by exception.");
            }
            finally
            {
                _finished.Set();
            }
        }


        protected abstract void Work(TState state);


        public virtual void Start(TState state)
        {
            if (_thread.Name == null)
            {
                Name = Name;    // Bakes the name as the default name.
            }
            _thread.Start(state);
        }
    }
}
