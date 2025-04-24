using Microsoft.AspNetCore.Mvc;

namespace TKPM_Project.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }

        public IActionResult ResetPassword(string token, string username)
        {
            ViewBag.Token = token;
            ViewBag.Username = username;
            return View();
        }
    }
}
