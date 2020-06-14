using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sidhe.LoginServer.Interfaces;

namespace Sidhe.LoginServer
{
    public class LoginAuthenticator : ILoginAuthenticator
    {
        [NotNull] private static readonly IReadOnlyDictionary<string, UserEntry> UserMap = new Dictionary<string, UserEntry> {
            { "foo", new UserEntry { Password = "bar", UserId = 777 } },
            { "eek", new UserEntry { Password = "ook", UserId = 123 } }
        };


        private class UserEntry
        {
            public string Password { get; set; }
            public int UserId { get; set; }
        }


        public int? Validate(string username, string password)
        {
            if (!string.IsNullOrWhiteSpace(username) && username.All(char.IsLetterOrDigit) &&
                UserMap.TryGetValue(username.ToLowerInvariant(), out var entry) &&
                string.Equals(password, entry.Password))
            {
                return entry.UserId;
            }

            return null;
        }
    }
}
