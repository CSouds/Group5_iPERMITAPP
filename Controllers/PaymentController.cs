// ============================================================
// PaymentController - Processes fee payments
// Simulates the OPS Common Payment Portal (CPP) for
// collecting application fees from RE users.
// ============================================================

using Group5_iPERMITAPP.Data;
using Group5_iPERMITAPP.Models;
using Group5_iPERMITAPP.Models.ViewModels;
using Group5_iPERMITAPP.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Group5_iPERMITAPP.Controllers
{
    /// <summary>
    /// PayFeeController - Manages the payment process through
    /// the simulated OPS Common Payment Portal.
    /// </summary>
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public PaymentController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // =====================================================
        // PAY FEE
        // =====================================================

        /// <summary>
        /// GET: Display the payment form (PayFeeForm).
        /// Informs the RE they will be directed to the OPS payment portal.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Pay(string requestNo)
        {
            var userId = HttpContext.Session.GetString("UserID");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId) || role != "RE")
                return RedirectToAction("Login", "Account");

            var permitRequest = await _context.PermitRequests
                .Include(pr => pr.RequestedPermit)
                .FirstOrDefaultAsync(pr => pr.RequestNo == requestNo && pr.REID == userId);

            if (permitRequest == null)
                return NotFound();

            // Check if already paid
            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.PermitRequestNo == requestNo && p.PaymentApproved);

            if (existingPayment != null)
            {
                TempData["InfoMessage"] = "This application has already been paid for.";
                return RedirectToAction("Dashboard", "PermitRequest");
            }

            var viewModel = new PaymentViewModel
            {
                PermitRequestNo = requestNo,
                Amount = permitRequest.PermitFee
            };

            return View(viewModel);
        }

        /// <summary>
        /// POST: Process the payment through the simulated OPS-CPP.
        /// On success, updates status to "Submitted" and triggers
        /// the AcknowledgeEO process.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(PaymentViewModel model)
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                var pr = await _context.PermitRequests
                    .FirstOrDefaultAsync(p => p.RequestNo == model.PermitRequestNo);
                if (pr != null) model.Amount = pr.PermitFee;
                return View(model);
            }

            var permitRequest = await _context.PermitRequests
                .Include(p => p.RequestedPermit)
                .Include(p => p.RequestedBy)
                .FirstOrDefaultAsync(pr => pr.RequestNo == model.PermitRequestNo);

            if (permitRequest == null)
                return NotFound();

            // Simulate OPS-CPP payment processing (always approved in this simulation)
            var paymentId = "PAY-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" +
                           new Random().Next(1000, 9999);

            var payment = new Payment
            {
                PaymentID = paymentId,
                PaymentDate = DateTime.Now,
                PaymentMethod = model.PaymentMethod,
                Last4DigitOfCard = model.CardNumber.Substring(model.CardNumber.Length - 4),
                CardHolderName = model.CardHolderName,
                PaymentApproved = true,
                PermitRequestNo = model.PermitRequestNo
            };

            _context.Payments.Add(payment);

            // Update status to "Submitted" (payment successful)
            var submittedStatus = new RequestStatus
            {
                PermitRequestStatus = "Submitted",
                Date = DateTime.Now,
                Description = $"Payment of ${permitRequest.PermitFee:F2} received. Application submitted for review.",
                PermitRequestNo = model.PermitRequestNo,
                UpdatedBy = "OPS-CPP"
            };

            _context.RequestStatuses.Add(submittedStatus);

            // ===== SEND EMAILS =====

            // Send confirmation email to RE
            if (permitRequest.RequestedBy != null)
            {
                var reEmailBody = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2>Payment Confirmation - iPERMIT Application</h2>
    <p>Dear {permitRequest.RequestedBy.ContactPersonName},</p>
    <p>Your payment for environmental permit application has been successfully processed.</p>

    <div style='background-color: #f0f0f0; padding: 15px; border-left: 4px solid #0d6efd;'>
        <strong>Application Details:</strong><br/>
        Request Number: {model.PermitRequestNo}<br/>
        Amount Paid: ${permitRequest.PermitFee:F2}<br/>
        Date: {DateTime.Now:MMMM dd, yyyy}<br/>
        Payment ID: {paymentId}
    </div>

    <p>Your application has been submitted for review by the Ministry's Environmental Officer and is now in the queue for evaluation.
    You will receive further updates as your application progresses through the approval process.</p>

    <p>You can track the status of your application by logging into the iPERMIT system at any time.</p>

    <p>If you have any questions, please contact our support team.</p>

    <p>Best regards,<br/>
    iPERMIT System<br/>
    Ontario Ministry of Environment</p>
</body>
</html>";

                await _emailService.SendEmailAsync(
                    permitRequest.RequestedBy.Email,
                    permitRequest.RequestedBy.ContactPersonName,
                    $"Payment Confirmation for iPERMIT Application {model.PermitRequestNo}",
                    reEmailBody
                );
            }

            // Send notification to EO (Acknowledge EO)
            var eoEmailBody = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2>New Application Submitted - iPERMIT</h2>
    <p>A new environmental permit application has been submitted and is ready for review.</p>

    <div style='background-color: #f0f0f0; padding: 15px; border-left: 4px solid #0d6efd;'>
        <strong>Application Details:</strong><br/>
        Request Number: {model.PermitRequestNo}<br/>
        Organization: {permitRequest.RequestedBy?.OrganizationName}<br/>
        Permit Type: {permitRequest.RequestedPermit?.PermitName}<br/>
        Application Fee: ${permitRequest.PermitFee:F2} (PAID)<br/>
        Activity: {permitRequest.ActivityDescription}<br/>
        Date Submitted: {DateTime.Now:MMMM dd, yyyy}
    </div>

    <p><a href='https://localhost:5001/EO/Review/{model.PermitRequestNo}' style='background-color: #0d6efd; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px; display: inline-block;'>Review Application</a></p>

    <p>Log in to the iPERMIT system to review and make a decision on this application.</p>
</body>
</html>";

            await _emailService.SendEmailAsync(
                "cosbdf@umsystem.edu",
                "Environmental Officer",
                $"New Application Ready for Review: {model.PermitRequestNo}",
                eoEmailBody
            );

            // Create email archive entries for logging
            var emailToEOArchive = new EmailArchive
            {
                EmailID = "EMAIL-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-EO",
                EmailDate = DateTime.Now,
                Reason = $"Payment confirmed for request {model.PermitRequestNo}. Application ready for review.",
                SentBy = "OPS-CPP",
                SentTo = "Environmental Officer",
                RecipientEmail = "eo@ministry.gov.on.ca",
                PermitRequestNo = model.PermitRequestNo
            };

            _context.EmailArchives.Add(emailToEOArchive);

            var emailToREArchive = new EmailArchive
            {
                EmailID = "EMAIL-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-RE",
                EmailDate = DateTime.Now,
                Reason = $"Payment confirmed. Your application {model.PermitRequestNo} has been submitted for review.",
                SentBy = "OPS-CPP",
                SentTo = permitRequest.RequestedBy?.ContactPersonName ?? "RE",
                RecipientEmail = permitRequest.RequestedBy?.Email ?? "",
                PermitRequestNo = model.PermitRequestNo
            };

            _context.EmailArchives.Add(emailToREArchive);

            await _context.SaveChangesAsync();

            return RedirectToAction("Success", new { requestNo = model.PermitRequestNo, paymentId = paymentId });
        }

        // =====================================================
        // PAYMENT SUCCESS (SuccessfulPaymentNoticeForm)
        // =====================================================

        /// <summary>
        /// Displays the successful payment notice popup.
        /// Informs the RE that payment was successful and
        /// control will return to the Ministry's web site.
        /// </summary>
        public async Task<IActionResult> Success(string requestNo, string paymentId)
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var payment = await _context.Payments
                .Include(p => p.PermitRequest)
                    .ThenInclude(pr => pr!.RequestedPermit)
                .FirstOrDefaultAsync(p => p.PaymentID == paymentId);

            if (payment == null) return NotFound();

            return View(payment);
        }
    }
}
