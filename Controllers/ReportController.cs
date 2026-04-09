// ============================================================
// ReportController - EO-only reports
// Generates on-screen reports showing all submitted permits
// and their statuses. Accessible only to the EO user.
// ============================================================

using Group5_iPERMITAPP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Group5_iPERMITAPP.Controllers
{
    /// <summary>
    /// Generates reports for the Environmental Officer.
    /// Reports are restricted to EO access only.
    /// </summary>
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Main report: All submitted permits and their current status.
        /// Accessible ONLY to the EO user.
        /// </summary>
        public async Task<IActionResult> PermitStatusReport()
        {
            // EO-only access check
            if (HttpContext.Session.GetString("UserRole") != "EO")
            {
                TempData["ErrorMessage"] = "Access denied. Reports are only accessible to the Environmental Officer.";
                return RedirectToAction("Login", "Account");
            }

            var allRequests = await _context.PermitRequests
                .Include(pr => pr.RequestedPermit)
                .Include(pr => pr.RequestedBy)
                .Include(pr => pr.Statuses)
                .Include(pr => pr.Payment)
                .Include(pr => pr.Decision)
                .Include(pr => pr.IssuedPermit)
                .OrderByDescending(pr => pr.DateOfRequest)
                .ToListAsync();

            // Summary statistics
            ViewBag.TotalApplications = allRequests.Count;
            ViewBag.TotalRevenue = allRequests
                .Where(pr => pr.Payment != null && pr.Payment.PaymentApproved)
                .Sum(pr => pr.PermitFee);

            var statusCounts = new Dictionary<string, int>();
            foreach (var request in allRequests)
            {
                var latestStatus = request.Statuses
                    .OrderByDescending(s => s.Date)
                    .FirstOrDefault()?.PermitRequestStatus ?? "Unknown";

                if (statusCounts.ContainsKey(latestStatus))
                    statusCounts[latestStatus]++;
                else
                    statusCounts[latestStatus] = 1;
            }
            ViewBag.StatusCounts = statusCounts;

            return View(allRequests);
        }

        /// <summary>
        /// Email archive report: Shows all logged emails.
        /// </summary>
        public async Task<IActionResult> EmailReport()
        {
            if (HttpContext.Session.GetString("UserRole") != "EO")
            {
                TempData["ErrorMessage"] = "Access denied. Reports are only accessible to the Environmental Officer.";
                return RedirectToAction("Login", "Account");
            }

            var emails = await _context.EmailArchives
                .OrderByDescending(e => e.EmailDate)
                .ToListAsync();

            return View(emails);
        }
    }
}
