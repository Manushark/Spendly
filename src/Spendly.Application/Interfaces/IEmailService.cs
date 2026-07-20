namespace Spendly.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink);

        /// <summary>Sends a budget alert email (80% warning or 100% exceeded).</summary>
        Task SendBudgetAlertEmailAsync(
            string toEmail,
            string category,
            decimal spent,
            decimal limit,
            decimal percentageUsed,
            bool isExceeded);

        /// <summary>Sends a weekly spending digest with totals and top categories.</summary>
        Task SendWeeklySummaryEmailAsync(
            string toEmail,
            decimal weekTotal,
            decimal monthTotal,
            string topCategory,
            int transactionCount);
    }
}
