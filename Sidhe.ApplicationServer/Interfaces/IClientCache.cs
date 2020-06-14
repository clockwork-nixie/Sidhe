using System.Diagnostics.CodeAnalysis;

namespace Sidhe.ApplicationServer.Interfaces
{
    public interface IClientCache
    {
        IApplicationClient AddOrReplace([NotNull] IApplicationClient client);
        IApplicationClient Find(int userId);
        bool Remove([NotNull] IApplicationClient client);
    }
}
