using JetBrains.Annotations;
using Sidhe.ApplicationServer.Model;
using Sidhe.Utilities.Interfaces;

namespace Sidhe.ApplicationServer.Interfaces
{
    public interface IWorld : IDispatcher<WorldWorkRequest>
    {
        Session BeginSession(int userId);
        void EndSession([NotNull] Session session);
        [NotNull] ClientResponse Execute([NotNull] ClientRequest request, Session session);
    }
}
