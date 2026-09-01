using Spendly.Application.DTOs.Auth;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using System.Security.Cryptography;

namespace Spendly.Application.UseCases.Auth
{
    public class ForgotPasswordUseCase
    {
        private readonly IUserRepository _userRepo;
        private readonly IPasswordResetTokenRepository _tokenRepo;
        private readonly IEmailService _emailService;

        public ForgotPasswordUseCase(
            IUserRepository userRepo,
            IPasswordResetTokenRepository tokenRepo,
            IEmailService emailService)
        {
            _userRepo = userRepo;
            _tokenRepo = tokenRepo;
            _emailService = emailService;
        }

        public async Task ExecuteAsync(ForgotPasswordDto dto, string baseUrl)
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            // Buscamos el usuario por email
            var user = await _userRepo.GetByEmailAsync(email);

            // Si el email no existe, salimos silenciosamente.
            // Nunca le decimos al cliente si el email está registrado o no
            // (esto previene el "user enumeration attack").
            if (user is null) return;

            // Generamos un token seguro de 64 caracteres hex
            var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

            var resetToken = PasswordResetToken.Create(user.Id, rawToken);
            await _tokenRepo.AddAsync(resetToken);

            // Construimos el link que irá en el email
            var resetLink = $"{baseUrl}/auth/reset-password?token={rawToken}";

            await _emailService.SendPasswordResetEmailAsync(email, resetLink);
        }
    }
}
