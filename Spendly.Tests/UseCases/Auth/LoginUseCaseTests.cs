using Moq;
using Spendly.Application.DTOs.Auth;
using Spendly.Application.Interfaces;
using Spendly.Application.UseCases.Auth;
using Spendly.Domain.Entities;
using Spendly.Domain.Exceptions;

namespace Spendly.Tests.UseCases.Auth;

public class LoginUseCaseTests
{
    private readonly Mock<IUserRepository>    _userRepo = new();
    private readonly Mock<IJwtTokenGenerator> _jwt      = new();

    private LoginUseCase Build() => new(_userRepo.Object, _jwt.Object);

    [Fact]
    public async Task ExecuteAsync_Should_ReturnToken_When_CredentialsAreValid()
    {
        // Arrange
        var user = User.Create("mario@test.com", BCrypt.Net.BCrypt.HashPassword("secret123"));

        _userRepo.Setup(r => r.GetByEmailAsync("mario@test.com")).ReturnsAsync(user);
        _jwt.Setup(j => j.GenerateToken(user)).Returns("fake-jwt-token");

        var useCase = Build();

        // Act
        var result = await useCase.ExecuteAsync(new LoginDto { Email = "mario@test.com", Password = "secret123" });

        // Assert
        Assert.Equal("fake-jwt-token", result.Token);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowInvalidCredentials_When_UserDoesNotExist()
    {
        // Arrange
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var useCase = Build();

        // Act + Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            useCase.ExecuteAsync(new LoginDto { Email = "ghost@test.com", Password = "pass" }));
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowInvalidCredentials_When_PasswordIsWrong()
    {
        // Arrange
        var user = User.Create("mario@test.com", BCrypt.Net.BCrypt.HashPassword("correct-password"));

        _userRepo.Setup(r => r.GetByEmailAsync("mario@test.com")).ReturnsAsync(user);

        var useCase = Build();

        // Act + Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            useCase.ExecuteAsync(new LoginDto { Email = "mario@test.com", Password = "wrong-password" }));
    }

    [Fact]
    public async Task ExecuteAsync_Should_NormalizeEmailToLowercase_Before_LookingUp()
    {
        // Arrange — el email llega con mayúsculas desde el frontend
        var user = User.Create("mario@test.com", BCrypt.Net.BCrypt.HashPassword("pass123"));

        _userRepo.Setup(r => r.GetByEmailAsync("mario@test.com")).ReturnsAsync(user);
        _jwt.Setup(j => j.GenerateToken(user)).Returns("token");

        var useCase = Build();

        // Act — se envía en mayúsculas
        var result = await useCase.ExecuteAsync(new LoginDto { Email = "MARIO@TEST.COM", Password = "pass123" });

        // Assert — debe funcionar igual porque se normaliza
        Assert.Equal("token", result.Token);
    }
}
