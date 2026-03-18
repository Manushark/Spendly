using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Configuration;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;

namespace Spendly.Infrastructure.Services
{
    /// <summary>
    /// Servicio para autenticación con Google OAuth 2.0
    /// </summary>
    public class GoogleAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtGenerator;

        public GoogleAuthService(
            IConfiguration configuration,
            IUserRepository userRepository,
            IJwtTokenGenerator jwtGenerator)
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _jwtGenerator = jwtGenerator;
        }

        /// <summary>
        /// Procesa el callback de Google y crea/actualiza usuario
        /// </summary>
        public async Task<AuthResult> HandleGoogleCallbackAsync(ClaimsPrincipal googleUser)
        {
            // Extraer información del usuario de Google
            var email = googleUser.FindFirstValue(ClaimTypes.Email);
            var name = googleUser.FindFirstValue(ClaimTypes.Name);
            var googleId = googleUser.FindFirstValue(ClaimTypes.NameIdentifier);
            var picture = googleUser.FindFirstValue("picture");

            if (string.IsNullOrEmpty(email))
            {
                return AuthResult.Failure("No se pudo obtener el email de Google");
            }

            // Buscar usuario existente
            var user = _userRepository.GetByEmail(email);

            if (user == null)
            {
                // Crear nuevo usuario con Google
                user = User.CreateWithGoogle(
                    email: email,
                    googleId: googleId,
                    displayName: name,
                    profilePicture: picture
                );

                _userRepository.Add(user);
            }
            else
            {
                // Actualizar información de Google si cambió
                user.UpdateGoogleInfo(googleId, name, picture);
                _userRepository.Update(user);
            }

            // Generar JWT token
            var token = _jwtGenerator.GenerateToken(user);

            return AuthResult.Success(token, user);
        }
    }

    public class AuthResult
    {
        public bool IsSuccess { get; private set; }
        public string Token { get; private set; }
        public User User { get; private set; }
        public string Error { get; private set; }

        private AuthResult(bool success, string token = null, User user = null, string error = null)
        {
            IsSuccess = success;
            Token = token;
            User = user;
            Error = error;
        }

        public static AuthResult Success(string token, User user)
            => new(true, token, user);

        public static AuthResult Failure(string error)
            => new(false, error: error);
    }
}
/// Nota: Este servicio asume que la configuración de Google OAuth ya está hecha en Startup.cs y que el controlador correspondiente redirige al frontend con el token generado.