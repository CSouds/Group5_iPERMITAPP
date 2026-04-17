// ============================================================
// AccountController - Manages RE and EO authentication
// Handles Registration, Login, Logout, Account Management,
// and Password Change for both RE and EO users.
// ============================================================

using Group5_iPERMITAPP.Data;
using Group5_iPERMITAPP.Models;
using Group5_iPERMITAPP.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Group5_iPERMITAPP.Controllers
{
    /// <summary>
    /// RegisterController - Manages the user registration and
    /// authentication process for RE and EO accounts.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // RE REGISTRATION
        // =====================================================

        /// <summary>
        /// GET: Display the RE registration form.
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        /// <summary>
        /// POST: Process RE registration - creates account,
        /// organization, contact person, address, and initial site.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check if user ID already exists
            if (await _context.REs.AnyAsync(r => r.ID == model.ID))
            {
                ModelState.AddModelError("ID", "This User ID is already taken.");
                return View(model);
            }

            // Check if email already exists
            if (await _context.REs.AnyAsync(r => r.Email == model.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            // Create the RE account
            var re = new RE
            {
                ID = model.ID,
                ContactPersonName = model.ContactPersonName,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                CreatedDate = DateTime.Now,
                Email = model.Email,
                OrganizationName = model.OrganizationName,
                OrganizationAddress = model.OrganizationAddress
            };

            _context.REs.Add(re);

            // Create the initial site
            var site = new RESite
            {
                SiteAddress = model.SiteAddress,
                SiteContactPerson = model.SiteContactPerson,
                REID = model.ID
            };

            _context.RESites.Add(site);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registration successful! Please log in.";
            return RedirectToAction("Login");
        }

        // =====================================================
        // LOGIN (RE and EO)
        // =====================================================

        /// <summary>
        /// GET: Display the login form.
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        /// <summary>
        /// POST: Validate credentials for both RE and EO users.
        /// Sets session variables for authenticated users.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Try EO login first
            var eo = await _context.EOs.FirstOrDefaultAsync(e => e.ID == model.UserID);
            if (eo != null && BCrypt.Net.BCrypt.Verify(model.Password, eo.Password))
            {
                HttpContext.Session.SetString("UserID", eo.ID);
                HttpContext.Session.SetString("UserName", eo.Name);
                HttpContext.Session.SetString("UserRole", "EO");
                HttpContext.Session.SetString("UserEmail", eo.Email);
                return RedirectToAction("Dashboard", "EO");
            }

            // Try RE login
            var re = await _context.REs.FirstOrDefaultAsync(r => r.ID == model.UserID);
            if (re != null && BCrypt.Net.BCrypt.Verify(model.Password, re.Password))
            {
                HttpContext.Session.SetString("UserID", re.ID);
                HttpContext.Session.SetString("UserName", re.ContactPersonName);
                HttpContext.Session.SetString("UserRole", "RE");
                return RedirectToAction("Dashboard", "PermitRequest");
            }

            ModelState.AddModelError("", "Invalid User ID or Password.");
            return View(model);
        }

        // =====================================================
        // LOGOUT
        // =====================================================

        /// <summary>
        /// Clears session and redirects to home page.
        /// </summary>
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // =====================================================
        // ACCOUNT MANAGEMENT
        // =====================================================

        /// <summary>
        /// GET: Display account details and management options.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var userId = HttpContext.Session.GetString("UserID");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login");

            if (role == "RE")
            {
                var re = await _context.REs
                    .Include(r => r.Sites)
                    .FirstOrDefaultAsync(r => r.ID == userId);

                if (re == null) return RedirectToAction("Login");
                return View(re);
            }

            return RedirectToAction("ChangePassword");
        }

        /// <summary>
        /// POST: Update RE account information.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(RE model)
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login");

            var re = await _context.REs.FindAsync(userId);
            if (re == null) return RedirectToAction("Login");

            // Check if the new email is already used by another RE
            if (re.Email != model.Email &&
                await _context.REs.AnyAsync(r => r.Email == model.Email && r.ID != userId))
            {
                TempData["ErrorMessage"] = "That email address is already registered to another account.";
                return RedirectToAction("Manage");
            }

            re.ContactPersonName = model.ContactPersonName;
            re.Email = model.Email;
            re.OrganizationName = model.OrganizationName;
            re.OrganizationAddress = model.OrganizationAddress;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Account updated successfully.";

            return RedirectToAction("Manage");
        }

        // =====================================================
        // CHANGE PASSWORD (RE and EO)
        // =====================================================

        /// <summary>
        /// GET: Display the change password form.
        /// </summary>
        [HttpGet]
        public IActionResult ChangePassword()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login");

            return View(new ChangePasswordViewModel());
        }

        /// <summary>
        /// POST: Process password change for both RE and EO.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = HttpContext.Session.GetString("UserID");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login");

            if (role == "EO")
            {
                var eo = await _context.EOs.FindAsync(userId);
                if (eo == null) return RedirectToAction("Login");

                if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, eo.Password))
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                    return View(model);
                }

                eo.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                await _context.SaveChangesAsync();
            }
            else // RE
            {
                var re = await _context.REs.FindAsync(userId);
                if (re == null) return RedirectToAction("Login");

                if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, re.Password))
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                    return View(model);
                }

                re.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction("ChangePassword");
        }

        // =====================================================
        // ADD SITE (RE only)
        // =====================================================

        /// <summary>
        /// GET: Display form to add a new site.
        /// </summary>
        [HttpGet]
        public IActionResult AddSite()
        {
            var userId = HttpContext.Session.GetString("UserID");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId) || role != "RE")
                return RedirectToAction("Login");

            return View(new RESite());
        }

        /// <summary>
        /// POST: Add a new site to the RE's account.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSite(RESite model)
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login");

            model.REID = userId;

            if (!ModelState.IsValid)
                return View(model);

            _context.RESites.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Site added successfully.";
            return RedirectToAction("Manage");
        }
    }
}
