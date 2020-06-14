using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Sidhe.ApplicationServer.Model;

namespace Sidhe.ApplicationServer.Interfaces
{
    public interface IApplicationClient : IDisposable
    {
        [NotNull] Task<bool> Initialise([NotNull] Action<IApplicationClient> onClose);
        [NotNull] Task Run([NotNull] Func<ClientRequest, Session, ClientResponse> dispatch, [NotNull] Session session);
        string LoginId { get; }
        void Terminate();
        int UserId { get; }
    }
}
