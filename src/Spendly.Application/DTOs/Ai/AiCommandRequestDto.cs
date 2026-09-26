using System.ComponentModel.DataAnnotations;

namespace Spendly.Application.DTOs.Ai
{
    public class AiCommandRequestDto
    {
        [Required]
        public string Prompt { get; set; } = string.Empty;
    }
}
