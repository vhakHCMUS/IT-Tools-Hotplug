using Microsoft.AspNetCore.Mvc;
using TKPM_Project.Controllers;

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
            var model = new ResetPasswordModel
            {
                Token = token,
                Username = username,
                Email = username // Since username is the email in this case
            };
            return View(model);
        }
    }
}
