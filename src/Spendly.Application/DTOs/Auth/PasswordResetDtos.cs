namespace Spendly.Application.DTOs.Auth
{
    public class ForgotPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string? ClientBaseUrl { get; set; }
    }

    public class ResetPasswordDto
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
