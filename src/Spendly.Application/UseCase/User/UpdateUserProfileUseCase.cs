using Spendly.Application.DTOs.User;
using Spendly.Application.Interfaces;
using Spendly.Domain.Exceptions;

namespace Spendly.Application.UseCases.User
{
    public class UpdateUserProfileUseCase
    {
        private readonly IUserRepository _userRepo;

        public UpdateUserProfileUseCase(IUserRepository userRepo) => _userRepo = userRepo;

        public async Task ExecuteAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _userRepo.GetByIdAsync(userId)
                ?? throw new InvalidDomainException("User not found.");

            user.UpdateProfile(dto.FullName, dto.PreferredCurrency, dto.TimeZone);
            await _userRepo.UpdateAsync(user);
        }
    }
}
