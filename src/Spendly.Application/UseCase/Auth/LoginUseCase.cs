using Spendly.Application.Interfaces;
using Spendly.Application.DTOs.Auth;
using Spendly.Domain.Exceptions;
using System.Security.Authentication;
using System.Net.Mail;


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

        public async Task<AuthResponseDto> ExecuteAsync(LoginDto dto)
        {
            var email = NormalizeEmail(dto.Email);
            var user = await _userRepository.GetByEmailAsync(email);

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

        private static string NormalizeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidCredentialsException("Invalid email or password.");

            var normalized = email.Trim().ToLowerInvariant();

            try
            {
                var address = new MailAddress(normalized);
                if (!string.Equals(address.Address, normalized, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidCredentialsException("Invalid email or password.");
            }
            catch (FormatException)
            {
                throw new InvalidCredentialsException("Invalid email or password.");
            }

            return normalized;
        }
    }
}
