using Spendly.Application.DTOs.Auth;
using Spendly.Application.Interfaces;
using Spendly.Domain.Exceptions;
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
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new InvalidDomainException("Email is required.");

            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new InvalidDomainException("Password is required.");

            if (dto.Password != dto.ConfirmPassword)
                throw new InvalidDomainException("Passwords do not match.");

            if (dto.Password.Length < 6)
                throw new InvalidDomainException("Password must be at least 6 characters.");

            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new InvalidDomainException("Email is already registered.");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var user = UserEntity.Create(dto.Email, passwordHash);

            await _userRepository.AddAsync(user);

            // Seed default categories for the new user
            await _categoryRepository.SeedDefaultsAsync(user.Id);

            var token = _jwt.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token
            };
        }
    }
}

