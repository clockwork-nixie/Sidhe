using System;
using JetBrains.Annotations;
using Sidhe.LoginServer.Interfaces;
using Sidhe.LoginServer.Models;

namespace Sidhe.LoginServer
{
    public class LoginService : ILoginService
    {
        [NotNull] private readonly ILoginRegistry _cache;
        [NotNull] private readonly ILoginAuthenticator _validator;


        public LoginService([NotNull] ILoginRegistry cache, [NotNull] ILoginAuthenticator validator)
        {
            _cache = cache;
            _validator = validator;
        }


        public LoginResponse Login(LoginRequest model)
        {
            if (!string.IsNullOrWhiteSpace(model.Username) &&
                !string.IsNullOrWhiteSpace(model.Password))
            {
                var userId = _validator.Validate(model.Username, model.Password);

                if (userId.HasValue && userId > 0)
                {
                    var loginId = Guid.NewGuid().ToString("N");

                    _cache[userId.Value] = loginId;

                    return new LoginResponse {
                        LoginId = loginId,
                        UserId = userId.Value
                    };
                }
            }

            return new LoginResponse();
        }

        
        public void Startup() { }
    }
}
