using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spendly.Application.DTOs.Expense
{
    public class UpdateExpenseDto
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
