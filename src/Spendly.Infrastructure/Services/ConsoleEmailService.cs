using Spendly.Application.Interfaces;

namespace Spendly.Infrastructure.Services
{
    /// <summary>
    /// Implementación de desarrollo: imprime los emails en la consola
    /// en lugar de enviarlos realmente.
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

        public Task SendBudgetAlertEmailAsync(
            string toEmail,
            string category,
            decimal spent,
            decimal limit,
            decimal percentageUsed,
            bool isExceeded)
        {
            var emoji   = isExceeded ? "🚨" : "⚠️";
            var subject = isExceeded
                ? $"Budget Exceeded — {category}"
                : $"Budget Warning (80%) — {category}";

            Console.WriteLine("─────────────────────────────────────────");
            Console.WriteLine($"[DEV EMAIL] To: {toEmail}");
            Console.WriteLine($"[DEV EMAIL] Subject: {subject}");
            Console.WriteLine($"[DEV EMAIL] {emoji} {category}: ${spent:N2} / ${limit:N2} ({percentageUsed:N0}%)");
            Console.WriteLine("─────────────────────────────────────────");
            return Task.CompletedTask;
        }

        public Task SendWeeklySummaryEmailAsync(
            string toEmail,
            decimal weekTotal,
            decimal monthTotal,
            string topCategory,
            int transactionCount)
        {
            Console.WriteLine("─────────────────────────────────────────");
            Console.WriteLine($"[DEV EMAIL] To: {toEmail}");
            Console.WriteLine($"[DEV EMAIL] Subject: Your Spendly Weekly Summary 📊");
            Console.WriteLine($"[DEV EMAIL] Week total:  ${weekTotal:N2}");
            Console.WriteLine($"[DEV EMAIL] Month total: ${monthTotal:N2}");
            Console.WriteLine($"[DEV EMAIL] Top category: {topCategory}");
            Console.WriteLine($"[DEV EMAIL] Transactions: {transactionCount}");
            Console.WriteLine("─────────────────────────────────────────");
            return Task.CompletedTask;
        }
    }
}
