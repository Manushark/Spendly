using Moq;
using Spendly.Application.DTOs.User;
using Spendly.Application.Interfaces;
using Spendly.Application.UseCases.User;
using Spendly.Domain.Exceptions;

namespace Spendly.Tests.UseCases.UserProfile;

public class UserUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepo = new();

    private static Domain.Entities.User MakeUser(int id = 1, string email = "user@test.com")
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("OldPass1!");
        var u = Domain.Entities.User.Create(email, hash);
        typeof(Domain.Entities.User).GetProperty(nameof(Domain.Entities.User.Id))!.SetValue(u, id);
        return u;
    }

    // ── ChangePasswordUseCase ─────────────────────────────────────────────────────

    [Fact]
    public async Task ChangePassword_Should_UpdateUser_When_ValidCredentials()
    {
        var user = MakeUser();
        _userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _userRepo.Setup(r => r.UpdateAsync(It.IsAny<Domain.Entities.User>())).Returns(Task.CompletedTask);

        var dto = new ChangePasswordDto
        {
            CurrentPassword    = "OldPass1!",
            NewPassword        = "NewPass1!",
            ConfirmNewPassword = "NewPass1!"
        };

        await new ChangePasswordUseCase(_userRepo.Object).ExecuteAsync(1, dto);

        _userRepo.Verify(r => r.UpdateAsync(It.IsAny<Domain.Entities.User>()), Times.Once);
    }

    [Fact]
    public async Task ChangePassword_Should_Throw_When_PasswordsDoNotMatch()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword    = "OldPass1!",
            NewPassword        = "NewPass1!",
            ConfirmNewPassword = "DifferentPass1!"
        };

        await Assert.ThrowsAsync<InvalidDomainException>(() =>
            new ChangePasswordUseCase(_userRepo.Object).ExecuteAsync(1, dto));
    }

    [Fact]
    public async Task ChangePassword_Should_Throw_When_NewPasswordTooShort()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword    = "OldPass1!",
            NewPassword        = "abc",
            ConfirmNewPassword = "abc"
        };

        await Assert.ThrowsAsync<InvalidDomainException>(() =>
            new ChangePasswordUseCase(_userRepo.Object).ExecuteAsync(1, dto));
    }

    [Fact]
    public async Task ChangePassword_Should_Throw_When_UserNotFound()
    {
        _userRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Domain.Entities.User?)null);

        var dto = new ChangePasswordDto
        {
            CurrentPassword    = "OldPass1!",
            NewPassword        = "NewPass123!",
            ConfirmNewPassword = "NewPass123!"
        };

        await Assert.ThrowsAsync<InvalidDomainException>(() =>
            new ChangePasswordUseCase(_userRepo.Object).ExecuteAsync(99, dto));
    }

    // ── GetUserProfileUseCase ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserProfile_Should_ReturnDto_When_UserExists()
    {
        var user = MakeUser(id: 1, email: "user@test.com");
        _userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var result = await new GetUserProfileUseCase(_userRepo.Object).ExecuteAsync(1);

        Assert.NotNull(result);
        Assert.Equal("user@test.com", result!.Email);
    }

    [Fact]
    public async Task GetUserProfile_Should_ThrowInvalidDomain_When_UserDoesNotExist()
    {
        _userRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Domain.Entities.User?)null);

        await Assert.ThrowsAsync<InvalidDomainException>(() =>
            new GetUserProfileUseCase(_userRepo.Object).ExecuteAsync(99));
    }

    // ── UpdateUserProfileUseCase ──────────────────────────────────────────────────

    [Fact]
    public async Task UpdateProfile_Should_UpdateUser_When_UserExists()
    {
        var user = MakeUser(id: 1);
        _userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _userRepo.Setup(r => r.UpdateAsync(It.IsAny<Domain.Entities.User>())).Returns(Task.CompletedTask);

        var dto = new UpdateProfileDto
        {
            FullName           = "Manuel",
            PreferredCurrency  = "USD",
            TimeZone           = "America/New_York"
        };

        await new UpdateUserProfileUseCase(_userRepo.Object).ExecuteAsync(1, dto);

        _userRepo.Verify(r => r.UpdateAsync(It.IsAny<Domain.Entities.User>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfile_Should_ThrowInvalidDomain_When_UserDoesNotExist()
    {
        _userRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Domain.Entities.User?)null);

        var dto = new UpdateProfileDto { FullName = "X", PreferredCurrency = "USD", TimeZone = "UTC" };

        await Assert.ThrowsAsync<InvalidDomainException>(() =>
            new UpdateUserProfileUseCase(_userRepo.Object).ExecuteAsync(99, dto));
    }
}
