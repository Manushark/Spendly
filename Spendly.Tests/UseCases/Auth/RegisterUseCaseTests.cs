using Moq;
using Spendly.Application.DTOs.Auth;
using Spendly.Application.Interfaces;
using Spendly.Application.UseCases.Auth;
using Spendly.Domain.Entities;
using Spendly.Domain.Exceptions;

namespace Spendly.Tests.UseCases.Auth;

public class RegisterUseCaseTests
{
    private readonly Mock<IUserRepository>    _userRepo     = new();
    private readonly Mock<IJwtTokenGenerator> _jwt          = new();
    private readonly Mock<ICategoryRepository> _categoryRepo = new();

    private RegisterUseCase Build() => new(_userRepo.Object, _jwt.Object, _categoryRepo.Object);

    private static RegisterDto ValidDto(string email = "new@test.com") => new()
    {
        Email           = email,
        Password        = "secure123",
        ConfirmPassword = "secure123"
    };

    [Fact]
    public async Task ExecuteAsync_Should_RegisterUser_And_ReturnToken_When_EmailIsNew()
    {
        // Arrange
        _userRepo.Setup(r => r.GetByEmailAsync("new@test.com")).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _categoryRepo.Setup(r => r.SeedDefaultsAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("registration-token");

        // Act
        var result = await Build().ExecuteAsync(ValidDto());

        // Assert
        Assert.Equal("registration-token", result.Token);
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _categoryRepo.Verify(r => r.SeedDefaultsAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowInvalidDomain_When_EmailAlreadyExists()
    {
        // Arrange
        var existing = User.Create("existing@test.com", "hash");
        _userRepo.Setup(r => r.GetByEmailAsync("existing@test.com")).ReturnsAsync(existing);

        var dto = ValidDto("existing@test.com");

        // Act + Assert
        await Assert.ThrowsAsync<InvalidDomainException>(() => Build().ExecuteAsync(dto));

        // El usuario NO debe guardarse si el email ya existe
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowInvalidDomain_When_PasswordsDoNotMatch()
    {
        // Arrange
        var dto = new RegisterDto
        {
            Email           = "new@test.com",
            Password        = "abc123",
            ConfirmPassword = "xyz999"  // diferente
        };

        // Act + Assert
        await Assert.ThrowsAsync<InvalidDomainException>(() => Build().ExecuteAsync(dto));
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowInvalidDomain_When_PasswordIsTooShort()
    {
        // Arrange — contraseña de 5 caracteres (mínimo es 6)
        var dto = new RegisterDto
        {
            Email           = "new@test.com",
            Password        = "abc12",
            ConfirmPassword = "abc12"
        };

        // Act + Assert
        await Assert.ThrowsAsync<InvalidDomainException>(() => Build().ExecuteAsync(dto));
    }
}
