using System.Collections.Concurrent;
using Sidhe.LoginServer.Interfaces;

namespace Sidhe.LoginServer
{
    public class LoginRegistry : ConcurrentDictionary<int, string>, ILoginRegistry { }
}
