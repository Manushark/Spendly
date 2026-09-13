using Spendly.Application.DTOs.Auth;
using Spendly.Application.Interfaces;

namespace Spendly.Application.UseCases.Auth
{
    public class DemoLoginUseCase
    {
        private readonly IDemoDataSeeder _demoSeeder;
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwt;

        public DemoLoginUseCase(
            IDemoDataSeeder demoSeeder,
            IUserRepository userRepository,
            IJwtTokenGenerator jwt)
        {
            _demoSeeder = demoSeeder;
            _userRepository = userRepository;
            _jwt = jwt;
        }

        public async Task<AuthResponseDto> ExecuteAsync()
        {
            var userId = await _demoSeeder.EnsureDemoUserAndDataAsync();
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("Could not load demo user account.");

            var token = _jwt.GenerateToken(user);
            return new AuthResponseDto
            {
                Token = token
            };
        }
    }
}
