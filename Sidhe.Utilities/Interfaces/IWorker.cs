using System;
using System.Threading;
using JetBrains.Annotations;

namespace Sidhe.Utilities.Interfaces
{
    public interface IWorker : IDisposable
    {
        [NotNull] WaitHandle FinishedHandle { get; }
        string Name { get; set; }
        void Start();
    }


    public interface IWorker<in TState> : IDisposable
    { 
        [NotNull] IWorker Bind(TState state);
        [NotNull] WaitHandle FinishedHandle { get; }
        string Name { get; set; }
        void Start(TState state);
    }
}
