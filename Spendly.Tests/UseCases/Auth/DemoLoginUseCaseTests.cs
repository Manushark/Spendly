using Moq;
using Spendly.Application.Interfaces;
using Spendly.Application.UseCases.Auth;
using Spendly.Domain.Entities;

namespace Spendly.Tests.UseCases.Auth;

public class DemoLoginUseCaseTests
{
    private readonly Mock<IDemoDataSeeder> _seeder = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IJwtTokenGenerator> _jwt = new();

    private DemoLoginUseCase Build() => new(_seeder.Object, _userRepo.Object, _jwt.Object);

    [Fact]
    public async Task ExecuteAsync_Should_EnsureDemoUser_And_ReturnJwtToken()
    {
        // Arrange
        const int demoUserId = 42;
        var demoUser = User.Create("demo@spendly.com", "fakehash");

        _seeder.Setup(s => s.EnsureDemoUserAndDataAsync()).ReturnsAsync(demoUserId);
        _userRepo.Setup(r => r.GetByIdAsync(demoUserId)).ReturnsAsync(demoUser);
        _jwt.Setup(j => j.GenerateToken(demoUser)).Returns("demo-jwt-token-xyz");

        var useCase = Build();

        // Act
        var response = await useCase.ExecuteAsync();

        // Assert
        Assert.NotNull(response);
        Assert.Equal("demo-jwt-token-xyz", response.Token);
        _seeder.Verify(s => s.EnsureDemoUserAndDataAsync(), Times.Once);
        _userRepo.Verify(r => r.GetByIdAsync(demoUserId), Times.Once);
        _jwt.Verify(j => j.GenerateToken(demoUser), Times.Once);
    }

    [Fact]
    public async Task ResetDemoDataUseCase_Should_CallSeeder_When_UserIsDemoUser()
    {
        // Arrange
        const int demoUserId = 42;
        var demoUser = User.Create("demo@spendly.com", "fakehash");
        _userRepo.Setup(r => r.GetByIdAsync(demoUserId)).ReturnsAsync(demoUser);
        _seeder.Setup(s => s.ResetDemoDataAsync(demoUserId)).Returns(Task.CompletedTask);

        var resetUseCase = new ResetDemoDataUseCase(_seeder.Object, _userRepo.Object);

        // Act
        await resetUseCase.ExecuteAsync(demoUserId);

        // Assert
        _seeder.Verify(s => s.ResetDemoDataAsync(demoUserId), Times.Once);
    }

    [Fact]
    public async Task ResetDemoDataUseCase_Should_ThrowUnauthorized_When_UserIsNotDemoUser()
    {
        // Arrange
        const int regularUserId = 99;
        var regularUser = User.Create("regular@spendly.com", "fakehash");
        _userRepo.Setup(r => r.GetByIdAsync(regularUserId)).ReturnsAsync(regularUser);

        var resetUseCase = new ResetDemoDataUseCase(_seeder.Object, _userRepo.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => resetUseCase.ExecuteAsync(regularUserId));
        _seeder.Verify(s => s.ResetDemoDataAsync(It.IsAny<int>()), Times.Never);
    }
}
