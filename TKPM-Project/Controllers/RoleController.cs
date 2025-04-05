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
            Console.WriteLine($"[GetRoles] Querying assign role {role} for: {userEmail}");

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                Console.WriteLine("User not found");
                return NotFound("User not found");
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                Console.WriteLine("role not exist");
                return BadRequest("Role does not exist");
            }

            await _userManager.AddToRoleAsync(user, role);

            Console.WriteLine("Assigned successfully");
            return Ok($"User {userEmail} assigned to role {role}");
        }

        [AllowAnonymous] // allow anyone to access this api, or delete it and use cookie
        [HttpGet("getRoles")]
        public async Task<IActionResult> GetUserRoles([FromQuery] string userEmail)
        {
            Console.WriteLine($"[GetRoles] Querying roles for: {userEmail}");

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                Console.WriteLine("User not found");
                return NotFound("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);

            Console.WriteLine($"Roles found: {string.Join(", ", roles)}");
            return Ok(roles);
        }
    }
}
