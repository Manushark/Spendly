using Spendly.Application.DTOs.User;
using Spendly.Application.Interfaces;
using Spendly.Domain.Exceptions;

namespace Spendly.Application.UseCases.User
{
    public class GetUserProfileUseCase
    {
        private readonly IUserRepository _userRepo;

        public GetUserProfileUseCase(IUserRepository userRepo) => _userRepo = userRepo;

        public async Task<UserProfileDto> ExecuteAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId)
                ?? throw new InvalidDomainException("User not found.");

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PreferredCurrency = user.PreferredCurrency,
                TimeZone = user.TimeZone,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
