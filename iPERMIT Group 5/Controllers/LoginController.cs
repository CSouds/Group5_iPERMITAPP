using iPERMIT_Group_5.Models;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace iPERMIT_Group_5.Controllers
{
    public class LoginController : Controller
    {
        private Group5_iPERMITDBEntities db = new Group5_iPERMITDBEntities();   // ← use your exact context name

        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public ActionResult Index(string username, string password)
        {
            // Hard-coded EO (rubric requirement)
            if (username == "eo@ministry.gov" && password == "password")
            {
                FormsAuthentication.SetAuthCookie("EO", false);
                Session["UserType"] = "EO";
                Session["UserName"] = "Environmental Officer";
                return RedirectToAction("Index", "Home");
            }

            // Real RE login from database
            var re = db.RE.FirstOrDefault(r => r.email == username && r.password == password);
            if (re != null)
            {
                FormsAuthentication.SetAuthCookie(re.ID, false);
                Session["UserType"] = "RE";
                Session["UserName"] = re.organizationName;
                Session["RE_ID"] = re.ID;
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid username or password";
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}