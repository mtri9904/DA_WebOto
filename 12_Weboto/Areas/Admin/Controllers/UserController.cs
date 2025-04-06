using _12_Weboto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace _12_Weboto.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Hiển thị danh sách người dùng
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new Dictionary<string, IList<string>>();

            foreach (var user in users)
            {
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        // Hiển thị form chỉnh sửa vai trò người dùng
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            ViewBag.Roles = roles;
            ViewBag.UserRoles = userRoles;
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, string FullName, string Address, string PhoneNumber, List<string> selectedRoles)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            // Cập nhật thông tin cá nhân
            user.FullName = FullName;
            user.Address = Address;
            user.PhoneNumber = PhoneNumber;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật thông tin người dùng.");
                return View(user);
            }

            // Cập nhật vai trò người dùng
            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeRolesResult.Succeeded)
            {
                ModelState.AddModelError("", "Lỗi khi xóa vai trò cũ.");
                return View(user);
            }

            var addRolesResult = await _userManager.AddToRolesAsync(user, selectedRoles);
            if (!addRolesResult.Succeeded)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật vai trò.");
                return View(user);
            }
            TempData["Success"] = "Cập nhật role người dùng thành công!";
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Tài khoản đã được xóa thành công.";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("Delete", user);
        }
    }
}
