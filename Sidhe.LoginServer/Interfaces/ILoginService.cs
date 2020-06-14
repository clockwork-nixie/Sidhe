using JetBrains.Annotations;
using Sidhe.LoginServer.Models;

namespace Sidhe.LoginServer.Interfaces
{
    public interface ILoginService
    {
        [NotNull] LoginResponse Login([NotNull] LoginRequest model);
        void Startup();
    }
}
