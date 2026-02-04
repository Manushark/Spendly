using Spendly.Application.Interfaces;
using Spendly.Application.DTOs.Auth;
using Spendly.Domain.Exceptions;
using System.Security.Authentication;


namespace Spendly.Application.UseCases.Auth
{
    public class LoginUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwt;

        public LoginUseCase(
            IUserRepository userRepository,
            IJwtTokenGenerator jwt)
        {
            _userRepository = userRepository;
            _jwt = jwt;
        }

        public AuthResponseDto Execute(LoginDto dto)
        {
            var user = _userRepository.GetByEmail(dto.Email);

            if (user is null)
                throw new InvalidCredentialsException("Invalid email or password.");

            if (!user.VerifyPassword(dto.Password))
                throw new InvalidCredentialsException("Invalid email or password.");

            var token = _jwt.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token
            };
        }
    }
}
