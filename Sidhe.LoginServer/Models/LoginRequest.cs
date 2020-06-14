namespace Sidhe.LoginServer.Models
{
    public class LoginRequest
    {
        public LoginRequest(string username, string password)
        {
            Password = password;
            Username = username;
        }


        public string Password { get; }
        public string Username { get; }
    }
}
