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

        public GoogleAuthController(
            GoogleAuthService googleAuthService,
            ILogger<GoogleAuthController> logger)
        {
            _googleAuthService = googleAuthService;
            _logger = logger;
        }

        /// <summary>
        /// Inicia el flujo de autenticación con Google
        /// GET: /api/auth/google/login
        /// </summary>
        [HttpGet("login")]
        public IActionResult GoogleLogin(string returnUrl = "/")
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback)),
                Items = { { "returnUrl", returnUrl } }
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
                // Autenticar con Google
                var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

                if (!authenticateResult.Succeeded)
                {
                    _logger.LogWarning("Google authentication failed");
                    return BadRequest(new { error = "Google authentication failed" });
                }

                // Procesar usuario y generar token
                var result = await _googleAuthService.HandleGoogleCallbackAsync(authenticateResult.Principal);

                if (!result.IsSuccess)
                {
                    _logger.LogError("Failed to process Google user: {Error}", result.Error);
                    return BadRequest(new { error = result.Error });
                }

                // Obtener returnUrl de las propiedades
                var returnUrl = authenticateResult.Properties?.Items["returnUrl"] ?? "/";

                // Redirigir al frontend con el token
                // En producción, usar tu dominio del frontend
                var frontendUrl = $"{Request.Scheme}://{Request.Host}{returnUrl}?token={result.Token}";

                return Redirect(frontendUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Google callback");
                return StatusCode(500, new { error = "An error occurred during authentication" });
            }
        }

        /// <summary>
        /// Endpoint para obtener información del usuario autenticado con Google
        /// GET: /api/auth/google/userinfo
        /// </summary>
        [HttpGet("userinfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
            {
                return Unauthorized();
            }

            var claims = authenticateResult.Principal.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            });

            return Ok(new
            {
                Email = authenticateResult.Principal.FindFirstValue(ClaimTypes.Email),
                Name = authenticateResult.Principal.FindFirstValue(ClaimTypes.Name),
                Picture = authenticateResult.Principal.FindFirstValue("picture"),
                Claims = claims
            });
        }
    }
}