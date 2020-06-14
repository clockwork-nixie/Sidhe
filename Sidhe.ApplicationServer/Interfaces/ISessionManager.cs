using Sidhe.ApplicationServer.Model;

namespace Sidhe.ApplicationServer.Interfaces
{
    public interface ISessionManager
    {
        void Clear();
        Session CreateSession(int userId, bool canReplace = false);
        bool DestroySession(int userId);
        Session FindSession(int userId);
    }
}
