using System.ComponentModel.DataAnnotations;

namespace Spendly.Application.DTOs.Category
{
    public class UpdateCategoryDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Icon { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Color { get; set; } = string.Empty;
    }
}
