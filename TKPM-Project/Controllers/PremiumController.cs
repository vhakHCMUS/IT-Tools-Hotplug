using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TKPM_Project.Models;
using TKPM_Project.Repositories;

namespace TKPM_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class PremiumController : ControllerBase
    {
        private readonly IUserPremiumRepository _premiumRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public PremiumController(IUserPremiumRepository premiumRepo, UserManager<ApplicationUser> userManager)
        {
            _premiumRepo = premiumRepo;
            _userManager = userManager;
        }

        [HttpPost("request")]
        public async Task<IActionResult> RequestPremium()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var existing = await _premiumRepo.GetByUserIdAsync(user.Id);
            if (existing != null && existing.ExpireDate > DateTime.UtcNow)
                return BadRequest("Bạn đã có Premium.");

            var premium = new UserPremium
            {
                UserId = user.Id,
                StartDate = DateTime.UtcNow,
                ExpireDate = DateTime.UtcNow.AddDays(7)
            };

            await _premiumRepo.AddAsync(premium);
            await _userManager.RemoveFromRolesAsync(user, await _userManager.GetRolesAsync(user));
            await _userManager.AddToRoleAsync(user, "Premium");

            return Ok("Bạn đã được cấp quyền Premium trong 7 ngày.");
        }

        [HttpGet("check")]
        public async Task<IActionResult> CheckPremium()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var active = await _premiumRepo.IsPremiumActiveAsync(user.Id);
            return Ok(new
            {
                Username = user.UserName,
                IsPremium = active
            });
        }

        // (Optional) Admin xem danh sách user còn Premium
        //[HttpGet("all")]
        //[Authorize(Roles = "Admin")]
        //public IActionResult GetAllActivePremiums()
        //{
        //    var users = _premiumRepo
        //        .GetAll()
        //        .Where(x => x.ExpireDate > DateTime.UtcNow)
        //        .Select(x => new
        //        {
        //            x.UserId,
        //            x.User.UserName,
        //            x.StartDate,
        //            x.ExpireDate
        //        });

        //    return Ok(users);
        //}
    }
}
