// ============================================================
// EOController - Environmental Officer operations
// Handles reviewing submitted applications, making decisions
// (approve/reject), issuing permits, and EO dashboard.
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
    /// ReviewSubmittedApplicationsController - Manages the EO's
    /// workflow for reviewing, deciding, and issuing permits.
    /// </summary>
    public class EOController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public EOController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        /// <summary>
        /// Helper: Verify EO is logged in.
        /// </summary>
        private bool IsEOAuthenticated()
        {
            return HttpContext.Session.GetString("UserRole") == "EO";
        }

        // =====================================================
        // EO DASHBOARD
        // =====================================================

        /// <summary>
        /// Displays the EO dashboard with summary statistics
        /// and recent applications.
        /// </summary>
        public async Task<IActionResult> Dashboard()
        {
            if (!IsEOAuthenticated())
                return RedirectToAction("Login", "Account");

            var allRequests = await _context.PermitRequests
                .Include(pr => pr.Statuses)
                .Include(pr => pr.RequestedPermit)
                .Include(pr => pr.RequestedBy)
                .ToListAsync();

            var viewModel = new EODashboardViewModel
            {
                TotalApplications = allRequests.Count,
                PendingReview = allRequests.Count(pr =>
                {
                    var latest = pr.Statuses.OrderByDescending(s => s.Date).FirstOrDefault();
                    return latest?.PermitRequestStatus == "Submitted" ||
                           latest?.PermitRequestStatus == "Being Reviewed";
                }),
                Approved = allRequests.Count(pr =>
                    pr.Statuses.Any(s => s.PermitRequestStatus == "Approved")),
                Rejected = allRequests.Count(pr =>
                    pr.Statuses.Any(s => s.PermitRequestStatus == "Rejected")),
                PermitsIssued = allRequests.Count(pr =>
                    pr.Statuses.Any(s => s.PermitRequestStatus == "Permit Issued")),
                RecentApplications = allRequests
                    .OrderByDescending(pr => pr.DateOfRequest)
                    .Take(10)
                    .ToList()
            };

            return View(viewModel);
        }

        // =====================================================
        // REVIEW SUBMITTED APPLICATIONS
        // =====================================================

        /// <summary>
        /// GET: Display all pending applications for review
        /// (ReviewSubmittedApplicationsForm).
        /// </summary>
        public async Task<IActionResult> ReviewApplications()
        {
            if (!IsEOAuthenticated())
                return RedirectToAction("Login", "Account");

            // Get applications with status "Submitted" (ready for review)
            var pendingRequests = await _context.PermitRequests
                .Include(pr => pr.RequestedPermit)
                .Include(pr => pr.RequestedBy)
                .Include(pr => pr.Payment)
                .Include(pr => pr.Statuses)
                .Where(pr => pr.Statuses.Any(s =>
                    s.PermitRequestStatus == "Submitted" ||
                    s.PermitRequestStatus == "Being Reviewed"))
                .ToListAsync();

            // Filter to only show those whose latest status is Submitted or Being Reviewed
            var filtered = pendingRequests.Where(pr =>
            {
                var latest = pr.Statuses.OrderByDescending(s => s.Date).FirstOrDefault();
                return latest?.PermitRequestStatus == "Submitted" ||
                       latest?.PermitRequestStatus == "Being Reviewed";
            }).ToList();

            return View(filtered);
        }

        /// <summary>
        /// GET: Review a specific application (ActivateCurrentRequest).
        /// Sets the status to "Being Reviewed" if not already.
        /// </summary>
        public async Task<IActionResult> Review(string id)
        {
            if (!IsEOAuthenticated())
                return RedirectToAction("Login", "Account");

            var permitRequest = await _context.PermitRequests
                .Include(pr => pr.RequestedPermit)
                .Include(pr => pr.RequestedBy)
                    .ThenInclude(re => re!.Sites)
                .Include(pr => pr.Payment)
                .Include(pr => pr.Statuses)
                .FirstOrDefaultAsync(pr => pr.RequestNo == id);

            if (permitRequest == null) return NotFound();

            // Update status to "Being Reviewed" if currently "Submitted"
            var latestStatus = permitRequest.Statuses
                .OrderByDescending(s => s.Date)
                .FirstOrDefault();

            if (latestStatus?.PermitRequestStatus == "Submitted")
            {
                var reviewStatus = new RequestStatus
                {
                    PermitRequestStatus = "Being Reviewed",
                    Date = DateTime.Now,
                    Description = "Application is being reviewed by the Environmental Officer.",
                    PermitRequestNo = id,
                    UpdatedBy = "EO"
                };
                _context.RequestStatuses.Add(reviewStatus);
                await _context.SaveChangesAsync();
            }

            ViewBag.Decision = new ReviewDecisionViewModel
            {
                PermitRequestNo = id
            };

            return View(permitRequest);
        }

        /// <summary>
        /// POST: Submit the EO's decision (approve or reject).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitDecision(ReviewDecisionViewModel model)
        {
            if (!IsEOAuthenticated())
                return RedirectToAction("Login", "Account");

            var eoId = HttpContext.Session.GetString("UserID")!;

            var permitRequest = await _context.PermitRequests
                .Include(pr => pr.RequestedBy)
                .Include(pr => pr.RequestedPermit)
                .FirstOrDefaultAsync(pr => pr.RequestNo == model.PermitRequestNo);

            if (permitRequest == null) return NotFound();

            // Create the decision record
            var decisionId = "DEC-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" +
                            new Random().Next(1000, 9999);

            var decision = new Decision
            {
                ID = decisionId,
                DateOfDecision = DateTime.Now,
                FinalDecision = model.FinalDecision,
                Description = model.Description,
                EOID = eoId,
                PermitRequestNo = model.PermitRequestNo
            };

            _context.Decisions.Add(decision);

            // Update application status
            var statusText = model.FinalDecision == "Approved" ? "Approved" : "Rejected";
            var statusDesc = model.FinalDecision == "Approved"
                ? "Application approved by the Environmental Officer."
                : $"Application rejected. Reason: {model.Description}";

            var newStatus = new RequestStatus
            {
                PermitRequestStatus = statusText,
                Date = DateTime.Now,
                Description = statusDesc,
                PermitRequestNo = model.PermitRequestNo,
                UpdatedBy = "EO"
            };

            _context.RequestStatuses.Add(newStatus);

            // ===== SEND EMAIL TO RE =====
            if (permitRequest.RequestedBy != null)
            {
                var decisionEmailBody = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2>Environmental Permit Application Decision</h2>
    <p>Dear {permitRequest.RequestedBy.ContactPersonName},</p>
    <p>Your environmental permit application has been reviewed by the Ministry's Environmental Officer.</p>

    <div style='background-color: {(model.FinalDecision == "Approved" ? "#d1e7dd" : "#f8d7da")}; padding: 15px; border-left: 4px solid {(model.FinalDecision == "Approved" ? "#0f5132" : "#842029")}; color: {(model.FinalDecision == "Approved" ? "#0f5132" : "#842029")}'>
        <strong style='font-size: 18px;'>Decision: {statusText.ToUpper()}</strong>
    </div>

    <div style='background-color: #f0f0f0; padding: 15px; margin-top: 15px; border-left: 4px solid #0d6efd;'>
        <strong>Application Details:</strong><br/>
        Request Number: {model.PermitRequestNo}<br/>
        Permit Type: {permitRequest.RequestedPermit?.PermitName}<br/>
        Activity: {permitRequest.ActivityDescription}
    </div>

    {(string.IsNullOrEmpty(model.Description) ? "" : $"<p><strong>Reason/Comments:</strong><br/>{model.Description}</p>")}

    {(model.FinalDecision == "Approved"
        ? "<p>Congratulations! Your permit application has been approved. The official permit will be issued and you will receive it shortly.</p>"
        : "<p>Your application was not approved at this time. You may resubmit an application after addressing the noted requirements.</p>")}

    <p>If you have questions about this decision, please contact the Ministry of Environment.</p>

    <p>Best regards,<br/>
    Environmental Officer<br/>
    Ontario Ministry of Environment</p>
</body>
</html>";
                await _emailService.SendEmailAsync(
                    permitRequest.RequestedBy.Email,
                    permitRequest.RequestedBy.ContactPersonName,
                    $"Decision on Environmental Permit Application {model.PermitRequestNo}",
                    decisionEmailBody
                );
            }

            // Archive the email
            var emailArchive = new EmailArchive
            {
                EmailID = "EMAIL-DEC-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                EmailDate = DateTime.Now,
                Reason = $"Your permit request {model.PermitRequestNo} has been {statusText.ToLower()}. {model.Description}",
                SentBy = "EO",
                SentTo = permitRequest.RequestedBy?.ContactPersonName ?? "RE",
                RecipientEmail = permitRequest.RequestedBy?.Email ?? "",
                PermitRequestNo = model.PermitRequestNo
            };

            _context.EmailArchives.Add(emailArchive);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Application {model.PermitRequestNo} has been {statusText.ToLower()}.";

            // If approved, redirect to issue permit
            if (model.FinalDecision == "Approved")
                return RedirectToAction("IssuePermit", new { id = model.PermitRequestNo });

            return RedirectToAction("ReviewApplications");
        }

        // =====================================================
        // ISSUE PERMIT
        // =====================================================

        /// <summary>
        /// GET: Display the issue permit form (IssuePermitForm).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> IssuePermit(string id)
        {
            if (!IsEOAuthenticated())
                return RedirectToAction("Login", "Account");

            var permitRequest = await _context.PermitRequests
                .Include(pr => pr.RequestedPermit)
                .Include(pr => pr.RequestedBy)
                .FirstOrDefaultAsync(pr => pr.RequestNo == id);

            if (permitRequest == null) return NotFound();

            return View(permitRequest);
        }

        /// <summary>
        /// POST: Issue the actual permit for an approved request.
        /// Creates the Permit record and updates status to "Permit Issued".
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmIssuePermit(string requestNo, string duration, string description)
        {
            if (!IsEOAuthenticated())
                return RedirectToAction("Login", "Account");

            var eoId = HttpContext.Session.GetString("UserID")!;

            var permitRequest = await _context.PermitRequests
                .Include(pr => pr.RequestedBy)
                .Include(pr => pr.RequestedPermit)
                .FirstOrDefaultAsync(pr => pr.RequestNo == requestNo);

            if (permitRequest == null) return NotFound();

            // Create the issued permit
            var permitId = "PRM-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" +
                          new Random().Next(1000, 9999);

            var permit = new Permit
            {
                PermitID = permitId,
                DateOfIssue = DateTime.Now,
                Duration = duration,
                Description = description,
                PermitRequestNo = requestNo,
                EOID = eoId,
                REID = permitRequest.REID
            };

            _context.Permits.Add(permit);

            // Update status to "Permit Issued"
            var issuedStatus = new RequestStatus
            {
                PermitRequestStatus = "Permit Issued",
                Date = DateTime.Now,
                Description = $"Permit {permitId} has been issued.",
                PermitRequestNo = requestNo,
                UpdatedBy = "EO"
            };

            _context.RequestStatuses.Add(issuedStatus);

            // ===== SEND EMAIL TO RE =====
            if (permitRequest.RequestedBy != null)
            {
                var permitEmailBody = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2>Environmental Permit Issued</h2>
    <p>Dear {permitRequest.RequestedBy.ContactPersonName},</p>
    <p>Your environmental permit has been officially issued by the Ministry of Environment.</p>

    <div style='background-color: #d1e7dd; padding: 15px; border-left: 4px solid #0f5132; color: #0f5132;'>
        <strong style='font-size: 18px;'>Permit ID: {permitId}</strong>
    </div>

    <div style='background-color: #f0f0f0; padding: 15px; margin-top: 15px; border-left: 4px solid #0d6efd;'>
        <strong>Permit Details:</strong><br/>
        Application Request: {requestNo}<br/>
        Permit Type: {permitRequest.RequestedPermit?.PermitName}<br/>
        Date of Issue: {DateTime.Now:MMMM dd, yyyy}<br/>
        Duration: {duration}<br/>
        Organization: {permitRequest.RequestedBy.OrganizationName}
    </div>

    <p><strong>Permit Description:</strong><br/>
    {description}</p>

    <p>This permit is now active and you may proceed with the approved activity subject to all conditions outlined in the permit.</p>

    <p>Please keep this email and the official permit for your records. You may be required to present it during inspections or audits.</p>

    <p>Best regards,<br/>
    Environmental Officer<br/>
    Ontario Ministry of Environment</p>
</body>
</html>";

                await _emailService.SendEmailAsync(
                    permitRequest.RequestedBy.Email,
                    permitRequest.RequestedBy.ContactPersonName,
                    $"Environmental Permit Issued: {permitId}",
                    permitEmailBody
                );
            }

            // Archive the email
            var emailArchive = new EmailArchive
            {
                EmailID = "EMAIL-ISS-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                EmailDate = DateTime.Now,
                Reason = $"Your environmental permit {permitId} has been issued for request {requestNo}.",
                SentBy = "EO",
                SentTo = permitRequest.RequestedBy?.ContactPersonName ?? "RE",
                RecipientEmail = permitRequest.RequestedBy?.Email ?? "",
                PermitRequestNo = requestNo
            };

            _context.EmailArchives.Add(emailArchive);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Permit {permitId} issued successfully.";
            return RedirectToAction("ReviewApplications");
        }

        // =====================================================
        // VIEW ALL APPLICATIONS (for EO)
        // =====================================================

        /// <summary>
        /// Lists all applications with their statuses for the EO.
        /// </summary>
        public async Task<IActionResult> AllApplications()
        {
            if (!IsEOAuthenticated())
                return RedirectToAction("Login", "Account");

            var allRequests = await _context.PermitRequests
                .Include(pr => pr.RequestedPermit)
                .Include(pr => pr.RequestedBy)
                .Include(pr => pr.Statuses)
                .Include(pr => pr.Payment)
                .Include(pr => pr.Decision)
                .Include(pr => pr.IssuedPermit)
                .OrderByDescending(pr => pr.DateOfRequest)
                .ToListAsync();

            return View(allRequests);
        }
    }
}
