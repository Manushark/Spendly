using Spendly.Application.DTOs.User;
using Spendly.Application.Interfaces;
using Spendly.Domain.Exceptions;

namespace Spendly.Application.UseCases.User
{
    public class ChangePasswordUseCase
    {
        private readonly IUserRepository _userRepo;

        public ChangePasswordUseCase(IUserRepository userRepo) => _userRepo = userRepo;

        public async Task ExecuteAsync(int userId, ChangePasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
                throw new InvalidDomainException("New passwords do not match.");

            if (dto.NewPassword.Length < 6)
                throw new InvalidDomainException("New password must be at least 6 characters.");

            var user = await _userRepo.GetByIdAsync(userId)
                ?? throw new InvalidDomainException("User not found.");

            var newHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.ChangePassword(dto.CurrentPassword, newHash);
            await _userRepo.UpdateAsync(user);
        }
    }
}
