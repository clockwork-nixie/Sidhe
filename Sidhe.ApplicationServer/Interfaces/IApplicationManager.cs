using System;
using System.Threading;
using JetBrains.Annotations;
using Sidhe.ApplicationServer.Model;

namespace Sidhe.ApplicationServer.Interfaces
{
    public interface IApplicationManager : IDisposable
    {
        Session BeginSession(int userId);
        void EndSession([NotNull] Session session);
        [NotNull] ClientResponse Execute([NotNull] ClientRequest request, [NotNull] Session session);
        [NotNull] WaitHandle FinishedHandle { get; }
        void QueueWork(WorldWorkRequest request);
        void Run();
        void Stop();
    }
}