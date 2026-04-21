using System.ComponentModel.DataAnnotations;

namespace Spendly.Application.DTOs.Income
{
    public class CreateIncomeDto
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(100)]
        public string Source { get; set; } = null!;

        [MaxLength(200)]
        public string? Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public bool IsRecurring { get; set; }

        [MaxLength(10)]
        public string Currency { get; set; } = "USD";
    }

    public class UpdateIncomeDto
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(100)]
        public string Source { get; set; } = null!;

        [MaxLength(200)]
        public string? Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public bool IsRecurring { get; set; }

        [MaxLength(10)]
        public string Currency { get; set; } = "USD";
    }

    public class IncomeResponseDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Source { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public bool IsRecurring { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
