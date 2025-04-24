using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TKPM_Project.Models;
using System.ComponentModel.DataAnnotations;

public class ForgotPasswordModel
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; }
}

public class ResetPasswordModel
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; }

    [Required(ErrorMessage = "New password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string NewPassword { get; set; }
}

namespace TKPM_Project.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Username))
                return BadRequest(new { message = "Vui lòng nhập username" });

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
                return BadRequest(new { message = "Người dùng không tồn tại" });

            // Ensure email is properly formatted
            string email = user.Email;
            if (!email.Contains("@"))
            {
                email = $"{email}@gmail.com";
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetUrl = $"http://localhost:5017/account/resetpassword?token={Uri.EscapeDataString(token)}&username={Uri.EscapeDataString(user.UserName)}";

            await _emailSender.SendEmailAsync(email, "Đặt lại mật khẩu",
                $"<p>Bạn đã yêu cầu đặt lại mật khẩu.</p><p><a href='{resetUrl}'>Bấm vào đây để đặt lại mật khẩu</a></p>");

            return Ok(new { message = "Email đặt lại mật khẩu đã được gửi" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            Console.WriteLine($"Received reset password request. Username: {model?.Username}, Token length: {model?.Token?.Length}, NewPassword length: {model?.NewPassword?.Length}");

            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.NewPassword))
            {
                Console.WriteLine("Missing required fields:");
                Console.WriteLine($"Username is empty: {string.IsNullOrEmpty(model.Username)}");
                Console.WriteLine($"Token is empty: {string.IsNullOrEmpty(model.Token)}");
                Console.WriteLine($"NewPassword is empty: {string.IsNullOrEmpty(model.NewPassword)}");
                return BadRequest(new { message = "Thiếu thông tin cần thiết" });
            }

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                Console.WriteLine($"User not found: {model.Username}");
                return BadRequest(new { message = "Người dùng không tồn tại" });
            }

            Console.WriteLine($"Found user: {user.UserName}, attempting to reset password");
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                Console.WriteLine("Password reset successful");
                await _signInManager.SignOutAsync();
                return Ok(new { message = "Đặt lại mật khẩu thành công" });
            }

            Console.WriteLine("Password reset failed. Errors:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"- {error.Code}: {error.Description}");
            }
            return BadRequest(new { message = "Không thể đặt lại mật khẩu", errors = result.Errors });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized("Invalid username or password");
            await _signInManager.SignInAsync(user, isPersistent: false); // tạo Cookie

            return Ok(new { message = "Login successful" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // Check if username is already an email
            string email;
            if (model.Username.Contains("@") && model.Username.Contains("."))
            {
                email = model.Username;
            }
            else
            {
                email = $"{model.Username}@gmail.com";
            }

            var user = new ApplicationUser 
            { 
                UserName = model.Username,
                Email = email
            };
            
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                return Ok(new { message = "User created successfully with User role" });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out" });
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegisterModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
