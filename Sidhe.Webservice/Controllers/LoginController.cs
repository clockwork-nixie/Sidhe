using System.Net;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sidhe.LoginServer.Interfaces;
using Sidhe.LoginServer.Models;

namespace Sidhe.Webservice.Controllers
{
    [ApiController, Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private readonly ILoginService _service;


        public class ApiLoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }


        public class ApiLoginResponse
        {
            public ApiLoginResponse([NotNull] LoginResponse response)
            {
                LoginToken = $"{response.UserId:X8}{response.LoginId}";
            }
            

            public string LoginToken { get; }
        }


        public LoginController([NotNull] ILogger<LoginController> logger, [NotNull] ILoginService service)
        {
            _service = service;
            _logger = logger;            
        }


        [HttpPost]
        public IActionResult Login([NotNull] ApiLoginRequest model)
        {
            if (_service == null)
            {
                return NotFound();
            }

            IActionResult result;

            if (string.IsNullOrWhiteSpace(model.Username))
            {
                result = BadRequest(new { Message = "Username cannot be empty." });
            }
            else if (string.IsNullOrWhiteSpace(model.Password))
            {
                result = BadRequest(new { Message = "Password cannot be empty." });
            }
            else
            {
                var request = new LoginRequest(model.Username, model.Password);
                var response = _service.Login(request);

                if (response.UserId <= 0)
                {
                    result = StatusCode((int) HttpStatusCode.Unauthorized);
                }
                else
                {
                    result = Ok(new ApiLoginResponse(response));
                }
            }

            return result;
        }
    }
}
