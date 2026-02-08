using Spendly.Application.DTOs.Auth;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Domain.Exceptions;

namespace Spendly.Application.UseCases.Auth
{
    public class RegisterUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwt;

        public RegisterUseCase(IUserRepository userRepository, IJwtTokenGenerator jwt)
        {
            _userRepository = userRepository;
            _jwt = jwt;
        }

        public AuthResponseDto Execute(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new InvalidDomainException("Email is required.");

            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new InvalidDomainException("Password is required.");

            if (dto.Password != dto.ConfirmPassword)
                throw new InvalidDomainException("Passwords do not match.");

            if (dto.Password.Length < 6)
                throw new InvalidDomainException("Password must be at least 6 characters.");

            var existingUser = _userRepository.GetByEmail(dto.Email);
            if (existingUser != null)
                throw new InvalidDomainException("Email is already registered.");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var user = User.Create(dto.Email, passwordHash);

            _userRepository.Add(user);

            var token = _jwt.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token
            };
        }
    }
}
