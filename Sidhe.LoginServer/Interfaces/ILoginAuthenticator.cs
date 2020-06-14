namespace Sidhe.LoginServer.Interfaces
{
    public interface ILoginAuthenticator
    {
        int? Validate(string username, string password);
    }
}
