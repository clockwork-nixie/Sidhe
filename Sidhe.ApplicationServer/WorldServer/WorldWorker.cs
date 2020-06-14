using System;
using DependencyInjection;
using JetBrains.Annotations;
using Sidhe.ApplicationServer.Interfaces;
using Sidhe.ApplicationServer.Model;
using Sidhe.Utilities;

namespace Sidhe.ApplicationServer.WorldServer
{
    public class WorldWorker : QueueWorker<WorldWorkRequest, IWorld>, IWorldWorker
    {
        public WorldWorker([NotNull] IFactory factory, [NotNull] IWorld world, [NotNull] IWorldWorkQueue queue)
            : base(factory, world, queue) { }


        public override void OnDispatched(WorldWorkRequest request, Exception exception = null)
        {
            base.OnDispatched(request, exception);
            request.Complete?.Set();
        }
    }
}