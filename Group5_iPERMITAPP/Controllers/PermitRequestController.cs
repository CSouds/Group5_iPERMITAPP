// ============================================================
// PermitRequestController - Manages environmental permits
// Handles permit application creation, submission, dashboard
// display, and status tracking for RE users.
// ============================================================

using Group5_iPERMITAPP.Data;
using Group5_iPERMITAPP.Models;
using Group5_iPERMITAPP.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Group5_iPERMITAPP.Controllers
{
    /// <summary>
    /// ManagePermitsController - Controls the process of
    /// requesting permits and following up on existing requests.
    /// </summary>
    public class PermitRequestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PermitRequestController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // RE DASHBOARD
        // =====================================================

        /// <summary>
        /// Displays the RE dashboard showing all permit requests
        /// and their current statuses.
        /// </summary>
        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.Session.GetString("UserID");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId) || role != "RE")
                return RedirectToAction("Login", "Account");

            var re = await _context.REs
                .Include(r => r.Sites)
                .FirstOrDefaultAsync(r => r.ID == userId);

            if (re == null) return RedirectToAction("Login", "Account");

            var permitRequests = await _context.PermitRequests
                .Include(pr => pr.RequestedPermit)
                .Include(pr => pr.Statuses)
                .Include(pr => pr.Payment)
                .Include(pr => pr.IssuedPermit)
                .Where(pr => pr.REID == userId)
                .OrderByDescending(pr => pr.DateOfRequest)
                .ToListAsync();

            // Get the latest status for each request
            var currentStatuses = new Dictionary<string, string>();
            foreach (var pr in permitRequests)
            {
                var latestStatus = pr.Statuses
                    .OrderByDescending(s => s.Date)
                    .FirstOrDefault();
                currentStatuses[pr.RequestNo] = latestStatus?.PermitRequestStatus ?? "Unknown";
            }

            var viewModel = new REDashboardViewModel
            {
                RE = re,
                PermitRequests = permitRequests,
                CurrentStatuses = currentStatuses
            };

            return View(viewModel);
        }

        // =====================================================
        // CREATE PERMIT REQUEST
        // =====================================================

        /// <summary>
        /// GET: Display the permit request form (PermitRequestForm).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = HttpContext.Session.GetString("UserID");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId) || role != "RE")
                return RedirectToAction("Login", "Account");

            // Load available permit types for the dropdown
            var permits = await _context.EnvironmentalPermits.ToListAsync();
            ViewBag.PermitTypes = new SelectList(permits, "PermitID", "PermitName");

            // Load RE sites
            var sites = await _context.RESites.Where(s => s.REID == userId).ToListAsync();
            ViewBag.Sites = new SelectList(sites, "SiteID", "SiteAddress");

            // Load permit fees as JSON for dynamic display
            ViewBag.PermitFees = permits.ToDictionary(p => p.PermitID, p => p.PermitFee);

            return View(new PermitRequestViewModel());
        }

        /// <summary>
        /// POST: Submit a new permit request.
        /// Creates the PermitRequest, sets initial status to "Pending Payment",
        /// and redirects to payment.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PermitRequestViewModel model)
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                var permits = await _context.EnvironmentalPermits.ToListAsync();
                ViewBag.PermitTypes = new SelectList(permits, "PermitID", "PermitName");
                var sites = await _context.RESites.Where(s => s.REID == userId).ToListAsync();
                ViewBag.Sites = new SelectList(sites, "SiteID", "SiteAddress");
                ViewBag.PermitFees = permits.ToDictionary(p => p.PermitID, p => p.PermitFee);
                return View(model);
            }

            // Get the permit fee from the selected environmental permit
            var envPermit = await _context.EnvironmentalPermits
                .FindAsync(model.EnvironmentalPermitID);

            if (envPermit == null)
            {
                ModelState.AddModelError("", "Selected permit type not found.");
                return View(model);
            }

            // Generate unique request number
            var requestNo = "REQ-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" +
                           new Random().Next(1000, 9999);

            // Create the permit request
            var permitRequest = new PermitRequest
            {
                RequestNo = requestNo,
                DateOfRequest = DateTime.Now,
                ActivityDescription = model.ActivityDescription,
                ActivityStartDate = model.ActivityStartDate,
                ActivityDuration = model.ActivityDuration,
                PermitFee = envPermit.PermitFee,
                REID = userId,
                EnvironmentalPermitID = model.EnvironmentalPermitID
            };

            _context.PermitRequests.Add(permitRequest);

            // Set initial status: "Pending Payment"
            var initialStatus = new RequestStatus
            {
                PermitRequestStatus = "Pending Payment",
                Date = DateTime.Now,
                Description = "Application created. Awaiting fee payment.",
                PermitRequestNo = requestNo,
                UpdatedBy = "System"
            };

            _context.RequestStatuses.Add(initialStatus);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Permit request {requestNo} created. Please proceed to payment.";
            return RedirectToAction("Pay", "Payment", new { requestNo = requestNo });
        }

        // =====================================================
        // VIEW REQUEST DETAILS
        // =====================================================

        /// <summary>
        /// Displays detailed information about a specific permit request,
        /// including its full status history.
        /// </summary>
        public async Task<IActionResult> Details(string id)
        {
            var userId = HttpContext.Session.GetString("UserID");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var permitRequest = await _context.PermitRequests
                .Include(pr => pr.RequestedPermit)
                .Include(pr => pr.Statuses)
                .Include(pr => pr.Payment)
                .Include(pr => pr.Decision)
                .Include(pr => pr.IssuedPermit)
                .Include(pr => pr.RequestedBy)
                .FirstOrDefaultAsync(pr => pr.RequestNo == id);

            if (permitRequest == null)
                return NotFound();

            // Ensure RE can only see their own requests
            if (role == "RE" && permitRequest.REID != userId)
                return Forbid();

            return View(permitRequest);
        }

        // =====================================================
        // STATUS HISTORY
        // =====================================================

        /// <summary>
        /// Displays the full status history of a permit request.
        /// </summary>
        public async Task<IActionResult> StatusHistory(string id)
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var statuses = await _context.RequestStatuses
                .Where(s => s.PermitRequestNo == id)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            ViewBag.RequestNo = id;
            return View(statuses);
        }
    }
}
