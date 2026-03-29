using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Spendly.Infrastructure.Services;
using System.Security.Claims;

namespace Spendly.Api.Controllers
{
    [ApiController]
    [Route("api/auth/google")]
    public class GoogleAuthController : ControllerBase
    {
        private readonly GoogleAuthService _googleAuthService;
        private readonly ILogger<GoogleAuthController> _logger;
        private readonly IConfiguration _configuration;

        public GoogleAuthController(
            GoogleAuthService googleAuthService,
            ILogger<GoogleAuthController> logger,
            IConfiguration configuration)
        {
            _googleAuthService = googleAuthService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Inicia el flujo de autenticación con Google
        /// GET: /api/auth/google/login
        /// </summary>
        [HttpGet("login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback)),
                Items = { { "scheme", GoogleDefaults.AuthenticationScheme } }
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Callback de Google después de autenticación exitosa
        /// GET: /api/auth/google/callback
        /// </summary>
        [HttpGet("callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            try
            {
                // Authenticate with the Cookies scheme (where Google stored the result)
                var authenticateResult = await HttpContext.AuthenticateAsync("Cookies");

                if (!authenticateResult.Succeeded)
                {
                    _logger.LogWarning("Google authentication failed: {Error}",
                        authenticateResult.Failure?.Message ?? "Unknown error");
                    
                    // Redirect to Web frontend with error
                    return Redirect("http://localhost:5242/Auth/Login?error=google_auth_failed");
                }

                // Process the user and generate JWT token
                var result = await _googleAuthService.HandleGoogleCallbackAsync(authenticateResult.Principal);

                if (!result.IsSuccess)
                {
                    _logger.LogError("Failed to process Google user: {Error}", result.Error);
                    return Redirect("http://localhost:5242/Auth/Login?error=google_processing_failed");
                }

                _logger.LogInformation("Google login successful for user {Email}, redirecting to Web frontend",
                    result.User?.Email);

                // Sign out of the temporary cookie scheme
                await HttpContext.SignOutAsync("Cookies");

                // Redirect to Web frontend with the JWT token
                var webFrontendUrl = $"http://localhost:5242/Auth/GoogleCallback?token={Uri.EscapeDataString(result.Token)}&email={Uri.EscapeDataString(result.User?.Email ?? "")}";
                return Redirect(webFrontendUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Google callback");
                return Redirect("http://localhost:5242/Auth/Login?error=internal_error");
            }
        }
    }
}