// ============================================================
// IEmailService - Email sending interface
// ============================================================

namespace Group5_iPERMITAPP.Services
{
    /// <summary>
    /// Interface for sending emails in the iPERMIT system.
    /// </summary>
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body);
    }
}
