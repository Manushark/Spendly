namespace Spendly.Application.DTOs.User
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string PreferredCurrency { get; set; } = "USD";
        public string TimeZone { get; set; } = "UTC";
        public DateTime CreatedAt { get; set; }
    }
}
