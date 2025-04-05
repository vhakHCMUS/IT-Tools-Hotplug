using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TKPM_Project.Models;

namespace TKPM_Project.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/roles")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole([FromQuery] string userEmail, [FromQuery] string role)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return NotFound("User not found");

            if (!await _roleManager.RoleExistsAsync(role))
                return BadRequest("Role does not exist");

            await _userManager.AddToRoleAsync(user, role);
            return Ok($"User {userEmail} assigned to role {role}");
        }

        [HttpGet("getRoles")]
        public async Task<IActionResult> GetUserRoles([FromQuery] string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return NotFound("User not found");

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }
    }
}
