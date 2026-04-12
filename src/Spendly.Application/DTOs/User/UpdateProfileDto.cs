using System.ComponentModel.DataAnnotations;

namespace Spendly.Application.DTOs.User
{
    public class UpdateProfileDto
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        [Required]
        [MaxLength(10)]
        public string PreferredCurrency { get; set; } = "USD";

        [Required]
        [MaxLength(50)]
        public string TimeZone { get; set; } = "UTC";
    }
}
