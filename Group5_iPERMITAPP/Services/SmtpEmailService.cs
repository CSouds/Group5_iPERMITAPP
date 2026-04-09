// ============================================================
// SmtpEmailService - Real SMTP email implementation
// Sends emails through a configured SMTP server
// ============================================================

using System.Net;
using System.Net.Mail;

namespace Group5_iPERMITAPP.Services
{
    /// <summary>
    /// Real email service using SMTP to send emails.
    /// Configure SMTP settings in appsettings.json
    /// </summary>
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body)
        {
            try
            {
                // Get SMTP settings from configuration
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var fromEmail = _configuration["Email:FromEmail"];
                var fromName = _configuration["Email:FromName"];
                var username = _configuration["Email:Username"];
                var password = _configuration["Email:Password"];
                var enableSsl = bool.Parse(_configuration["Email:EnableSSL"] ?? "true");

                // Validate configuration
                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(fromEmail))
                {
                    _logger.LogWarning("SMTP settings not configured. Email not sent to {toEmail}", toEmail);
                    return false;
                }

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.EnableSsl = enableSsl;

                    // Only set credentials if username is provided
                    if (!string.IsNullOrEmpty(username))
                    {
                        client.Credentials = new NetworkCredential(username, password);
                    }

                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress(fromEmail, fromName ?? "iPERMIT System");
                        message.To.Add(new MailAddress(toEmail, toName));
                        message.Subject = subject;
                        message.Body = body;
                        message.IsBodyHtml = true;

                        await client.SendMailAsync(message);

                        _logger.LogInformation("Email sent successfully to {toEmail} with subject '{subject}'", toEmail, subject);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {toEmail}", toEmail);
                return false;
            }
        }
    }

    /// <summary>
    /// Fallback email service for development/testing.
    /// Logs emails to console instead of sending them.
    /// </summary>
    public class ConsoleEmailService : IEmailService
    {
        private readonly ILogger<ConsoleEmailService> _logger;

        public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
        {
            _logger = logger;
        }

        public Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body)
        {
            _logger.LogInformation("=== EMAIL (Console Mode - Not Actually Sent) ===");
            _logger.LogInformation("To: {toName} <{toEmail}>", toName, toEmail);
            _logger.LogInformation("Subject: {subject}", subject);
            _logger.LogInformation("Body:\n{body}", body);
            _logger.LogInformation("================================================");

            return Task.FromResult(true);
        }
    }
}
