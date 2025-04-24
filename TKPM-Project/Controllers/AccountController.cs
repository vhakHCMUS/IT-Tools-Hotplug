using Microsoft.AspNetCore.Mvc;
using TKPM_Project.Models;
using TKPM_Project.Services;
using System.Threading.Tasks;

namespace TKPM_Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly IEmailSender _emailSender;

        public AccountController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

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
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            var model = new TKPM_Project.Models.ResetPasswordModel
            {
                Token = token,
                Username = username
            };

            return View(model);
        }
    }
}
