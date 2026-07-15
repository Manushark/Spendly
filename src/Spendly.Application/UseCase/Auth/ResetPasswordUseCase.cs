using Spendly.Application.DTOs.Auth;
using Spendly.Application.Interfaces;
using Spendly.Domain.Exceptions;

namespace Spendly.Application.UseCases.Auth
{
    public class ResetPasswordUseCase
    {
        private readonly IPasswordResetTokenRepository _tokenRepo;
        private readonly IUserRepository _userRepo;

        public ResetPasswordUseCase(
            IPasswordResetTokenRepository tokenRepo,
            IUserRepository userRepo)
        {
            _tokenRepo = tokenRepo;
            _userRepo = userRepo;
        }

        public async Task ExecuteAsync(ResetPasswordDto dto)
        {
            // Validamos que las contraseñas coincidan
            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                throw new InvalidDomainException("Password is required.");

            if (dto.NewPassword != dto.ConfirmPassword)
                throw new InvalidDomainException("Passwords do not match.");

            if (dto.NewPassword.Length < 6)
                throw new InvalidDomainException("Password must be at least 6 characters.");

            // Buscamos el token en la BD
            var resetToken = await _tokenRepo.GetByTokenAsync(dto.Token)
                ?? throw new InvalidDomainException("Reset link is invalid or has already been used.");

            // Validamos: no expirado, no usado
            if (!resetToken.IsValid())
                throw new InvalidDomainException("Reset link has expired. Please request a new one.");

            // Buscamos al usuario dueño del token
            var user = await _userRepo.GetByIdAsync(resetToken.UserId)
                ?? throw new InvalidDomainException("User not found.");

            // Actualizamos la contraseña
            var newHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.SetPasswordHash(newHash);
            await _userRepo.UpdateAsync(user);

            // Invalidamos el token para que no pueda reutilizarse
            resetToken.MarkAsUsed();
            await _tokenRepo.UpdateAsync(resetToken);
        }
    }
}
