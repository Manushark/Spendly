using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spendly.Application.DTOs.Expense
{
    // DTO for creating a new Expense
    public class CreateExpenseDto
    {
        [Required]
        [Range(1, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = null!;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = null!;
    }
}
