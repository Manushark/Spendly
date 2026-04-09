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
        private readonly RegisterUseCase _registerUseCase;

        public AuthController(LoginUseCase loginUseCase, RegisterUseCase registerUseCase)
        {
            _loginUseCase = loginUseCase;
            _registerUseCase = registerUseCase;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _loginUseCase.ExecuteAsync(dto);
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _registerUseCase.ExecuteAsync(dto);
            return Ok(result);
        }
    }
}
