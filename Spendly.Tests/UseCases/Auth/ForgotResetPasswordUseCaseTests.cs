using Moq;
using Spendly.Application.DTOs.Auth;
using Spendly.Application.Interfaces;
using Spendly.Application.UseCases.Auth;
using Spendly.Domain.Entities;

namespace Spendly.Tests.UseCases.Auth;

public class ForgotResetPasswordUseCaseTests
{
    private readonly Mock<IUserRepository>               _userRepo  = new();
    private readonly Mock<IPasswordResetTokenRepository> _tokenRepo = new();
    private readonly Mock<IEmailService>                 _email     = new();

    private static Domain.Entities.User MakeUser(int id = 1, string email = "user@test.com")
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("Pass1234!");
        var u = Domain.Entities.User.Create(email, hash);
        typeof(Domain.Entities.User).GetProperty(nameof(Domain.Entities.User.Id))!.SetValue(u, id);
        return u;
    }

    // ── ForgotPasswordUseCase ─────────────────────────────────────────────────────

    [Fact]
    public async Task ForgotPassword_Should_SendEmail_When_EmailExists()
    {
        var user = MakeUser();
        _userRepo.Setup(r => r.GetByEmailAsync("user@test.com")).ReturnsAsync(user);
        _tokenRepo.Setup(r => r.AddAsync(It.IsAny<PasswordResetToken>())).Returns(Task.CompletedTask);
        _email.Setup(e => e.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
              .Returns(Task.CompletedTask);

        var dto = new ForgotPasswordDto { Email = "user@test.com" };

        await new ForgotPasswordUseCase(_userRepo.Object, _tokenRepo.Object, _email.Object)
            .ExecuteAsync(dto, "https://spendly.app");

        _tokenRepo.Verify(r => r.AddAsync(It.IsAny<PasswordResetToken>()), Times.Once);
        _email.Verify(e => e.SendPasswordResetEmailAsync("user@test.com", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ForgotPassword_Should_SilentlyExit_When_EmailDoesNotExist()
    {
        // Anti-enumeration: no exception, no email sent
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((Domain.Entities.User?)null);

        var dto = new ForgotPasswordDto { Email = "ghost@test.com" };

        await new ForgotPasswordUseCase(_userRepo.Object, _tokenRepo.Object, _email.Object)
            .ExecuteAsync(dto, "https://spendly.app");

        _tokenRepo.Verify(r => r.AddAsync(It.IsAny<PasswordResetToken>()), Times.Never);
        _email.Verify(e => e.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // ── ResetPasswordUseCase ──────────────────────────────────────────────────────

    [Fact]
    public async Task ResetPassword_Should_UpdatePassword_When_TokenIsValid()
    {
        var user = MakeUser(id: 1);
        var token = PasswordResetToken.Create(1, "VALIDTOKEN");

        _tokenRepo.Setup(r => r.GetByTokenAsync("VALIDTOKEN")).ReturnsAsync(token);
        _userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _userRepo.Setup(r => r.UpdateAsync(It.IsAny<Domain.Entities.User>())).Returns(Task.CompletedTask);
        _tokenRepo.Setup(r => r.UpdateAsync(It.IsAny<PasswordResetToken>())).Returns(Task.CompletedTask);

        var dto = new ResetPasswordDto
        {
            Token = "VALIDTOKEN",
            NewPassword = "NewPass123!",
            ConfirmPassword = "NewPass123!"
        };

        await new ResetPasswordUseCase(_tokenRepo.Object, _userRepo.Object).ExecuteAsync(dto);

        _userRepo.Verify(r => r.UpdateAsync(It.IsAny<Domain.Entities.User>()), Times.Once);
        _tokenRepo.Verify(r => r.UpdateAsync(It.IsAny<PasswordResetToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_Should_ThrowInvalidDomain_When_TokenNotFound()
    {
        _tokenRepo.Setup(r => r.GetByTokenAsync("BADTOKEN")).ReturnsAsync((PasswordResetToken?)null);

        var dto = new ResetPasswordDto
        {
            Token = "BADTOKEN",
            NewPassword = "NewPass123!",
            ConfirmPassword = "NewPass123!"
        };

        await Assert.ThrowsAsync<Domain.Exceptions.InvalidDomainException>(() =>
            new ResetPasswordUseCase(_tokenRepo.Object, _userRepo.Object).ExecuteAsync(dto));
    }

    [Fact]
    public async Task ResetPassword_Should_Throw_When_PasswordsDoNotMatch()
    {
        var dto = new ResetPasswordDto
        {
            Token = "TOKEN",
            NewPassword = "NewPass123!",
            ConfirmPassword = "DifferentPass!"
        };

        // Should throw even before fetching token
        await Assert.ThrowsAsync<Domain.Exceptions.InvalidDomainException>(() =>
            new ResetPasswordUseCase(_tokenRepo.Object, _userRepo.Object).ExecuteAsync(dto));
    }
}
