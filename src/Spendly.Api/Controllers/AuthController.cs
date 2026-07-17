using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Spendly.Application.DTOs.Auth;
using Spendly.Application.UseCases.Auth;
using Spendly.Api.Security;

namespace Spendly.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly LoginUseCase _loginUseCase;
        private readonly RegisterUseCase _registerUseCase;
        private readonly ForgotPasswordUseCase _forgotPasswordUseCase;
        private readonly ResetPasswordUseCase _resetPasswordUseCase;

        public AuthController(
            LoginUseCase loginUseCase,
            RegisterUseCase registerUseCase,
            ForgotPasswordUseCase forgotPasswordUseCase,
            ResetPasswordUseCase resetPasswordUseCase)
        {
            _loginUseCase = loginUseCase;
            _registerUseCase = registerUseCase;
            _forgotPasswordUseCase = forgotPasswordUseCase;
            _resetPasswordUseCase = resetPasswordUseCase;
        }

        [EnableRateLimiting(RateLimitPolicies.Auth)]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _loginUseCase.ExecuteAsync(dto);
            return Ok(result);
        }

        [EnableRateLimiting(RateLimitPolicies.Auth)]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _registerUseCase.ExecuteAsync(dto);
            return Ok(result);
        }

        [EnableRateLimiting(RateLimitPolicies.Auth)]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            // Construimos la URL base del Web app para el link del email
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            await _forgotPasswordUseCase.ExecuteAsync(dto, baseUrl);

            // Siempre respondemos igual — no revelamos si el email existe
            return Ok(new { message = "If that email is registered, you will receive a reset link shortly." });
        }

        [EnableRateLimiting(RateLimitPolicies.Auth)]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            await _resetPasswordUseCase.ExecuteAsync(dto);
            return Ok(new { message = "Password has been reset successfully." });
        }
    }
}
