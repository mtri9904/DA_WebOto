using _12_Weboto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace _12_Weboto.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(SignInManager<ApplicationUser> signInManager,ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var totalCars = _context.Cars.Count();
            var totalBrands = _context.Brands.Count();
            var totalUsers = _context.Users.Count();
            var totalNews = _context.News.Count();
            var totalOrders = _context.Orders.Count();

            ViewBag.TotalCars = totalCars;
            ViewBag.TotalBrands = totalBrands;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalNews = totalNews;
            ViewBag.TotalOrders = totalOrders;

            var user = await _userManager.GetUserAsync(User); // Lấy người dùng hiện tại
            if (user != null)
            {
                ViewData["FullName"] = user.FullName; // Gán FullName vào ViewData
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/Identity/Account/Login"); // Chuyển hướng đến login mặc định của Identity
        }
    }
}
