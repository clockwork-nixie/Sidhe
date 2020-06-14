using System;
using System.Collections.Generic;
using System.Threading;
using DependencyInjection;
using JetBrains.Annotations;
using Sidhe.ApplicationServer.Interfaces;
using Sidhe.ApplicationServer.Model;
using Sidhe.Utilities.Interfaces;

namespace Sidhe.ApplicationServer
{
    public class ApplicationManager : Dependency, IApplicationManager
    {
        [NotNull] private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        [NotNull] private readonly CancellationTokenSource _cancellation;
        [NotNull] private readonly ManualResetEventSlim _finished = new ManualResetEventSlim();
        [NotNull] private readonly IQueueWorkerSet<WorldWorkRequest, IWorldWorker, IWorldWorkQueue> _queueSet;
        [NotNull] private readonly IWorld _world;



        public ApplicationManager([NotNull] IFactory factory) : base(factory)
        {
            Logger.Info("Creating application manager.");

            _cancellation = new CancellationTokenSource();
            _world = GetInstance<IWorld>();

            var settings = GetInstance<IApplicationSettings>();

            _queueSet = GetInstance<IQueueWorkerSet<WorldWorkRequest, IWorldWorker, IWorldWorkQueue>>();

            _queueSet.Configure(
                Math.Max(1, (int)settings.WorldQueueCount),
                Math.Max(1, (int)settings.WorldWorkersPerQueue), 
                _cancellation.Token,
                queue => GetInstance<IWorldWorker, IWorld, IWorldWorkQueue>(_world, queue),
                request => request.RequestId);
        }


        public Session BeginSession(int userId) => _world.BeginSession(userId);


        
        public void Dispose()
        {
            Logger.Info("Disposing application manager.");

            _queueSet.Dispose();
            _cancellation.Dispose();
        }


        public void EndSession(Session session) => _world.EndSession(session);
        
        
        public ClientResponse Execute(ClientRequest request, Session session) => _world.Execute(request, session);


        public WaitHandle FinishedHandle => _finished.WaitHandle;


        public void QueueWork(WorldWorkRequest request)
        {
            _queueSet.QueueRequest(request);
            request?.Complete?.Wait();
        }


        public void Run()
        {
            Logger.Info("Starting application manager.");

            _queueSet.Start();

            var waitHandles = new List<WaitHandle>(_queueSet.WaitHandles) {_cancellation.Token.WaitHandle};

            WaitHandle.WaitAll(waitHandles.ToArray());
            _finished.Set();

            Logger.Info("Application manager stopped.");
        }


        public void Stop()
        {
            Logger.Info("Stopping application manager.");
            _cancellation.Cancel();
        }
    }
}
