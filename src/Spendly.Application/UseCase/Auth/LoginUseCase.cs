using Spendly.Application.DTOs.Auth;
using Spendly.Application.Interfaces;
using Spendly.Domain.Exceptions;
using Spendly.Infrastructure.Security;

namespace Spendly.Application.UseCases.Auth
{
    public class LoginUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtTokenGenerator _jwt;

        public LoginUseCase(
            IUserRepository userRepository,
            JwtTokenGenerator jwt)
        {
            _userRepository = userRepository;
            _jwt = jwt;
        }

        public AuthResponseDto Execute(LoginDto dto)
        {
            var user = _userRepository.GetByEmail(dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new DomainException("Invalid credentials.");

            var token = _jwt.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token
            };
        }
    }
}
