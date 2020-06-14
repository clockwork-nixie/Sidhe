using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Sidhe.ApplicationServer.Interfaces
{
    public interface IApplicationService
    {
        void Recycle();
        Task RunClient([NotNull] IApplicationClient client);
        [NotNull] IApplicationSettings Settings { get; }
        void Startup();
        void Shutdown();
    }
}
