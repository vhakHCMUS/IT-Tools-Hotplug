using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TKPM_Project.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TKPM_Project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Hiển thị danh sách người dùng
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
            var allRoles = await _roleManager.Roles
                .Select(r => r.Name)
                .Where(r => !string.IsNullOrEmpty(r)) // Lọc bỏ null hoặc chuỗi rỗng
                .ToListAsync();

            var model = new EditUserViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName ?? string.Empty,
                Roles = userRoles.ToList(),
                AllRoles = allRoles,
                SelectedRoles = userRoles.ToArray()
            };

            return View(model);
        }

        // POST: Xử lý cập nhật thông tin người dùng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                model.SelectedRoles = model.SelectedRoles ?? Array.Empty<string>();
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
            user.FullName = model.FullName ?? string.Empty;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                model.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                model.SelectedRoles = model.SelectedRoles ?? Array.Empty<string>();
                return View(model);
            }

            // Cập nhật vai trò
            var currentRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = (model.SelectedRoles ?? Array.Empty<string>())
                .Where(r => !string.IsNullOrEmpty(r)) // Lọc bỏ null hoặc chuỗi rỗng
                .ToArray();
            var rolesToAdd = selectedRoles.Except(currentRoles).ToList();
            var rolesToRemove = currentRoles.Except(selectedRoles).ToList();

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
                    model.SelectedRoles = selectedRoles;
                    return View(model);
                }
            }

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
                    model.SelectedRoles = selectedRoles;
                    return View(model);
                }
            }

            return RedirectToAction(nameof(UserManager));
        }
    }

    // ViewModel cho danh sách người dùng
    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public IList<string> Roles { get; set; }
    }

    // ViewModel cho chỉnh sửa người dùng
    public class EditUserViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public List<string> Roles { get; set; } = new List<string>(); // Khởi tạo mặc định
        public List<string> AllRoles { get; set; } = new List<string>(); // Khởi tạo mặc định
        public string[] SelectedRoles { get; set; } // Vai trò được chọn từ form
    }
}