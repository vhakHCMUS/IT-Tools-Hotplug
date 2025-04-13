using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TKPM_Project.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace TKPM_Project.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        // Admin methods with [Authorize] attribute
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserManager()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRolesViewModel.Add(new UserRolesViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    Roles = roles
                });
            }

            return View(userRolesViewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            var model = new EditUserViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Roles = userRoles.ToList(),
                AllRoles = allRoles,
                SelectedRoles = userRoles.ToArray() // Initialize with current roles
            };

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin người dùng
            user.UserName = model.UserName;
            user.Email = model.Email;
            user.FullName = model.FullName;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                model.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                return View(model);
            }

            // Cập nhật vai trò
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Handle null SelectedRoles (no checkboxes selected)
            var selectedRoles = model.SelectedRoles ?? new string[0];

            var rolesToAdd = selectedRoles.Where(r => !currentRoles.Contains(r)).ToList();
            var rolesToRemove = currentRoles.Where(r => !selectedRoles.Contains(r)).ToList();

            if (rolesToAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    foreach (var error in addResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    model.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                    return View(model);
                }
            }

            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    foreach (var error in removeResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    model.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                    return View(model);
                }
            }

            return RedirectToAction(nameof(UserManager));
        }

        // User profile methods - accessible to authenticated users
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            // Get current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                Email = user.Email,
                FullName = user.FullName
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Get current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Update only allowed fields
            user.Email = model.Email;
            user.FullName = model.FullName;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // Update normalized email if email changed
            await _userManager.UpdateNormalizedEmailAsync(user);

            // Add success message
            TempData["StatusMessage"] = "Your profile has been updated successfully!";

            return RedirectToAction(nameof(Profile));
        }

        // Premium Request methods
        [Authorize]
        public async Task<IActionResult> PremiumRequest()
        {
            // Get current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Check if user is already premium - check directly from the database
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains("Premium"))
            {
                ViewBag.Message = "You already have Premium status!";
                return View();
            }

            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PremiumRequest(PremiumRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Get current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Check if user is already premium
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains("Premium"))
            {
                ViewBag.Message = "You already have Premium status!";
                return View();
            }

            // Verify the premium password
            if (model.Password == "123456")
            {
                // Add user to Premium role
                var result = await _userManager.AddToRoleAsync(user, "Premium");
                
                if (result.Succeeded)
                {
                    // Re-sign in the user to refresh their claims/roles
                    await _signInManager.SignOutAsync();
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    
                    ViewBag.Success = true;
                    ViewBag.Message = "Congratulations! You are now a Premium user!";
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("Password", "Incorrect password. Please try again.");
            }

            return View(model);
        }
    }

    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public IList<string> Roles { get; set; }
    }

    public class EditUserViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public List<string> AllRoles { get; set; }
        public string[] SelectedRoles { get; set; }
    }

    public class ProfileViewModel
    {
        public string Email { get; set; }
        public string FullName { get; set; }
    }

    public class PremiumRequestViewModel
    {
        public string Password { get; set; }
    }
}