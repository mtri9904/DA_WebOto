using _12_Weboto.Models; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.Mvc; 
using Microsoft.EntityFrameworkCore; 
using System.Linq;
using System.Threading.Tasks; // Ghi chú: Sử dụng Task để hỗ trợ các phương thức bất đồng bộ

namespace _12_Weboto.Areas.Admin.Controllers 
{
    [Area("Admin")] 
    [Authorize(Roles = SD.Role_Admin)] 
    public class UserController : Controller 
    {
        private readonly UserManager<ApplicationUser> _userManager; 
        private readonly RoleManager<IdentityRole> _roleManager; 

        // Ghi chú: Constructor, tiêm phụ thuộc UserManager và RoleManager
        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager; 
            _roleManager = roleManager; 
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync(); // Lấy danh sách tất cả người dùng bất đồng bộ
            var userRoles = new Dictionary<string, IList<string>>(); //Tạo từ điển để lưu vai trò của từng người dùng

            foreach (var user in users) // Ghi chú: Duyệt qua từng người dùng
            {
                userRoles[user.Id] = await _userManager.GetRolesAsync(user); 
            }

            ViewBag.UserRoles = userRoles; 
            return View(users);
        }


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


            user.FullName = FullName; 
            user.Address = Address; 
            user.PhoneNumber = PhoneNumber; 

            var updateResult = await _userManager.UpdateAsync(user); 
            if (!updateResult.Succeeded) 
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật thông tin người dùng."); 
                return View(user); 
            }


            var currentRoles = await _userManager.GetRolesAsync(user); 
            var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, currentRoles); 
            if (!removeRolesResult.Succeeded)
            {
                ModelState.AddModelError("", "Lỗi khi xóa vai trò cũ.");
                return View(user);
            }

            var addRolesResult = await _userManager.AddToRolesAsync(user, selectedRoles); // Ghi chú: Thêm vai trò mới từ danh sách đã chọn
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
                ModelState.AddModelError("", error.Description); // Ghi chú: Thêm từng lỗi vào ModelState
            }

            return View("Delete", user); 
        }
    }
}