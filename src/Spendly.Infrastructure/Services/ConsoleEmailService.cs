using Spendly.Application.Interfaces;

namespace Spendly.Infrastructure.Services
{
    /// <summary>
    /// Implementación de desarrollo: muestra el link de reset en la consola
    /// en lugar de enviar un email real.
    /// Para producción, reemplazar con SmtpEmailService o SendGridEmailService.
    /// </summary>
    public class ConsoleEmailService : IEmailService
    {
        public Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            Console.WriteLine("─────────────────────────────────────────");
            Console.WriteLine($"[DEV EMAIL] To: {toEmail}");
            Console.WriteLine($"[DEV EMAIL] Subject: Reset your Spendly password");
            Console.WriteLine($"[DEV EMAIL] Reset link: {resetLink}");
            Console.WriteLine("─────────────────────────────────────────");
            return Task.CompletedTask;
        }
    }
}
