using Microsoft.AspNetCore.Mvc;
using Spendly.Application.DTOs.Auth;
using Spendly.Application.UseCases.Auth;

namespace Spendly.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly LoginUseCase _loginUseCase;

        public AuthController(LoginUseCase loginUseCase)
        {
            _loginUseCase = loginUseCase;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            var result = _loginUseCase.Execute(dto);
            return Ok(result);
        }
    }
}

