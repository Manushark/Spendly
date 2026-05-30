using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Spendly.Web.Services;

namespace Spendly.Web.Contracts.Expenses
{
    public class ExpenseDto
    {
        public int Id { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        public List<int> TagIds { get; set; } = [];
        public List<TagDto> Tags { get; set; } = [];
    }
}
