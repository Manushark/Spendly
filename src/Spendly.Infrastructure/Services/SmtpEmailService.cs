using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Spendly.Application.Interfaces;

namespace Spendly.Infrastructure.Services
{
    /// <summary>
    /// Implementación real de IEmailService usando SMTP (Gmail, Outlook, SendGrid, etc.).
    /// Se activa cuando la sección "Smtp" en appsettings tiene Host, Username y Password.
    /// </summary>
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(SmtpSettings settings, ILogger<SmtpEmailService> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private SmtpClient BuildClient()
        {
            return new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl    = _settings.EnableSsl
            };
        }

        private MailMessage BaseMessage(string toEmail, string subject)
        {
            var msg = new MailMessage
            {
                From       = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject    = subject,
                IsBodyHtml = true
            };
            msg.To.Add(toEmail);
            return msg;
        }

        private async Task SendAsync(MailMessage message)
        {
            try
            {
                using var client = BuildClient();
                await client.SendMailAsync(message);
                _logger.LogInformation("[SMTP] Email sent to {To}: {Subject}", message.To, message.Subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SMTP] Failed to send email to {To}: {Subject}", message.To, message.Subject);
            }
        }

        // ── IEmailService implementation ─────────────────────────────────────

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var msg = BaseMessage(toEmail, "Reset your Spendly password");
            msg.Body = $@"
<div style='font-family:sans-serif;max-width:520px;margin:auto;padding:32px;background:#0f1117;color:#e2e8f0;border-radius:12px;'>
  <h2 style='color:#6c5ce7;'>🔐 Reset your Spendly password</h2>
  <p>We received a request to reset your password. Click the button below to continue:</p>
  <a href='{resetLink}' style='display:inline-block;margin:20px 0;padding:12px 28px;background:#6c5ce7;color:#fff;border-radius:8px;text-decoration:none;font-weight:600;'>
    Reset Password
  </a>
  <p style='color:#94a3b8;font-size:0.85rem;'>This link expires in 1 hour. If you didn't request this, ignore this email.</p>
  <hr style='border-color:#2d3748;margin:24px 0;'/>
  <p style='color:#64748b;font-size:0.8rem;'>Spendly — Personal Finance Tracker</p>
</div>";
            await SendAsync(msg);
        }

        public async Task SendBudgetAlertEmailAsync(
            string toEmail, string category,
            decimal spent, decimal limit,
            decimal percentageUsed, bool isExceeded)
        {
            var emoji   = isExceeded ? "🚨" : "⚠️";
            var color   = isExceeded ? "#e74c3c" : "#f39c12";
            var subject = isExceeded
                ? $"Budget Exceeded — {category}"
                : $"Budget Warning (80%) — {category}";

            var msg  = BaseMessage(toEmail, subject);
            msg.Body = $@"
<div style='font-family:sans-serif;max-width:520px;margin:auto;padding:32px;background:#0f1117;color:#e2e8f0;border-radius:12px;'>
  <h2 style='color:{color};'>{emoji} Budget Alert — {category}</h2>
  <p>Your <strong>{category}</strong> budget has reached <strong>{percentageUsed:N0}%</strong>.</p>
  <div style='background:#1a202c;border-radius:8px;padding:20px;margin:20px 0;'>
    <p style='margin:0;font-size:1.2rem;'><strong>Spent:</strong> ${spent:N2}</p>
    <p style='margin:8px 0 0;color:#94a3b8;'><strong>Limit:</strong> ${limit:N2}</p>
    <div style='background:#2d3748;border-radius:4px;height:8px;margin-top:12px;'>
      <div style='background:{color};border-radius:4px;height:8px;width:{Math.Min((double)percentageUsed, 100):N0}%;'></div>
    </div>
  </div>
  <a href='https://spendly.azurewebsites.net/budgets' style='display:inline-block;padding:12px 28px;background:#6c5ce7;color:#fff;border-radius:8px;text-decoration:none;font-weight:600;'>
    View Budgets
  </a>
  <hr style='border-color:#2d3748;margin:24px 0;'/>
  <p style='color:#64748b;font-size:0.8rem;'>Spendly — Personal Finance Tracker</p>
</div>";
            await SendAsync(msg);
        }

        public async Task SendWeeklySummaryEmailAsync(
            string toEmail,
            decimal weekTotal, decimal monthTotal,
            string topCategory, int transactionCount)
        {
            var msg  = BaseMessage(toEmail, "Your Spendly Weekly Summary 📊");
            msg.Body = $@"
<div style='font-family:sans-serif;max-width:520px;margin:auto;padding:32px;background:#0f1117;color:#e2e8f0;border-radius:12px;'>
  <h2 style='color:#6c5ce7;'>📊 Your Weekly Spending Summary</h2>
  <p>Here's how you did this past week:</p>
  <div style='background:#1a202c;border-radius:8px;padding:20px;margin:20px 0;'>
    <table style='width:100%;border-collapse:collapse;'>
      <tr>
        <td style='padding:8px 0;color:#94a3b8;'>Week Total</td>
        <td style='padding:8px 0;text-align:right;font-size:1.2rem;font-weight:700;color:#6c5ce7;'>${weekTotal:N2}</td>
      </tr>
      <tr>
        <td style='padding:8px 0;color:#94a3b8;'>Month-to-Date</td>
        <td style='padding:8px 0;text-align:right;color:#a29bfe;'>${monthTotal:N2}</td>
      </tr>
      <tr>
        <td style='padding:8px 0;color:#94a3b8;'>Top Category</td>
        <td style='padding:8px 0;text-align:right;'>{topCategory}</td>
      </tr>
      <tr>
        <td style='padding:8px 0;color:#94a3b8;'>Transactions</td>
        <td style='padding:8px 0;text-align:right;'>{transactionCount}</td>
      </tr>
    </table>
  </div>
  <a href='https://spendly.azurewebsites.net/dashboard' style='display:inline-block;padding:12px 28px;background:#6c5ce7;color:#fff;border-radius:8px;text-decoration:none;font-weight:600;'>
    View Dashboard
  </a>
  <hr style='border-color:#2d3748;margin:24px 0;'/>
  <p style='color:#64748b;font-size:0.8rem;'>Spendly — Personal Finance Tracker · You can turn off these emails in Settings → Notifications</p>
</div>";
            await SendAsync(msg);
        }
    }

    /// <summary>Modelo de configuración SMTP leído desde appsettings.json → "Smtp" section.</summary>
    public class SmtpSettings
    {
        public string Host      { get; set; } = string.Empty;
        public int    Port      { get; set; } = 587;
        public bool   EnableSsl { get; set; } = true;
        public string Username  { get; set; } = string.Empty;
        public string Password  { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName  { get; set; } = "Spendly";
    }
}
