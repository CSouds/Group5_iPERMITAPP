// ============================================================
// HomeController - Landing page and navigation
// ============================================================

using Microsoft.AspNetCore.Mvc;

namespace Group5_iPERMITAPP.Controllers
{
    /// <summary>
    /// Handles the home/landing page of the iPERMIT application.
    /// </summary>
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
