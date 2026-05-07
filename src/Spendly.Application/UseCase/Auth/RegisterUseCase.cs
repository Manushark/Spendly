using Spendly.Application.DTOs.Auth;
using Spendly.Application.Interfaces;
using Spendly.Domain.Exceptions;
using System.Net.Mail;
using UserEntity = Spendly.Domain.Entities.User;

namespace Spendly.Application.UseCases.Auth
{
    public class RegisterUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwt;
        private readonly ICategoryRepository _categoryRepository;

        public RegisterUseCase(
            IUserRepository userRepository,
            IJwtTokenGenerator jwt,
            ICategoryRepository categoryRepository)
        {
            _userRepository = userRepository;
            _jwt = jwt;
            _categoryRepository = categoryRepository;
        }

        public async Task<AuthResponseDto> ExecuteAsync(RegisterDto dto)
        {
            var email = NormalizeEmail(dto.Email);

            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new InvalidDomainException("Password is required.");

            if (dto.Password != dto.ConfirmPassword)
                throw new InvalidDomainException("Passwords do not match.");

            if (dto.Password.Length < 6)
                throw new InvalidDomainException("Password must be at least 6 characters.");

            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
                throw new InvalidDomainException("Email is already registered.");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var user = UserEntity.Create(email, passwordHash);

            await _userRepository.AddAsync(user);

            // Seed default categories for the new user
            await _categoryRepository.SeedDefaultsAsync(user.Id);

            var token = _jwt.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token
            };
        }

        private static string NormalizeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidDomainException("Email is required.");

            var normalized = email.Trim().ToLowerInvariant();

            if (normalized.Length > 256)
                throw new InvalidDomainException("Email is too long.");

            try
            {
                var address = new MailAddress(normalized);
                if (!string.Equals(address.Address, normalized, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDomainException("Email format is invalid.");
            }
            catch (FormatException)
            {
                throw new InvalidDomainException("Email format is invalid.");
            }

            return normalized;
        }
    }
}

