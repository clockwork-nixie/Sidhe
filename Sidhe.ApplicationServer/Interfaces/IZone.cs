using System.Diagnostics.CodeAnalysis;
using Sidhe.ApplicationServer.Model;

namespace Sidhe.ApplicationServer.Interfaces
{
    public interface IZone
    {
        void Remove(int userId);
        ClientLocation[] UpdateLocation([NotNull] ClientLocation location, [NotNull] Session session);
    }
}
