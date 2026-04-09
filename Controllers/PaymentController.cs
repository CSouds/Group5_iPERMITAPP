// ============================================================
// PaymentController - Processes fee payments
// Simulates the OPS Common Payment Portal (CPP) for
// collecting application fees from RE users.
// ============================================================

using Group5_iPERMITAPP.Data;
using Group5_iPERMITAPP.Models;
using Group5_iPERMITAPP.Models.ViewModels;
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

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
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

            // Create email archive entry (Acknowledge EO)
            // This declares to the EO that the RE's payment is complete
            var emailToEO = new EmailArchive
            {
                EmailID = "EMAIL-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-EO",
                EmailDate = DateTime.Now,
                Reason = $"Payment confirmed for request {model.PermitRequestNo}. Application ready for review.",
                SentBy = "OPS-CPP",
                SentTo = "EO",
                RecipientEmail = "eo@ministry.gov.on.ca",
                PermitRequestNo = model.PermitRequestNo
            };

            _context.EmailArchives.Add(emailToEO);

            // Send confirmation email to RE
            var emailToRE = new EmailArchive
            {
                EmailID = "EMAIL-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-RE",
                EmailDate = DateTime.Now,
                Reason = $"Payment confirmed. Your application {model.PermitRequestNo} has been submitted for review.",
                SentBy = "OPS-CPP",
                SentTo = permitRequest.RequestedBy?.ContactPersonName ?? "RE",
                RecipientEmail = permitRequest.RequestedBy?.Email ?? "",
                PermitRequestNo = model.PermitRequestNo
            };

            _context.EmailArchives.Add(emailToRE);

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
