using Spendly.Application.Interfaces;
using Spendly.Domain.Exceptions;

namespace Spendly.Application.UseCases.Auth
{
    public class ResetDemoDataUseCase
    {
        private readonly IDemoDataSeeder _demoSeeder;
        private readonly IUserRepository _userRepository;

        public ResetDemoDataUseCase(
            IDemoDataSeeder demoSeeder,
            IUserRepository userRepository)
        {
            _demoSeeder = demoSeeder;
            _userRepository = userRepository;
        }

        public async Task ExecuteAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !string.Equals(user.Email, "demo@spendly.com", StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Reset demo data is only authorized for the demo account.");

            await _demoSeeder.ResetDemoDataAsync(userId);
        }
    }
}
