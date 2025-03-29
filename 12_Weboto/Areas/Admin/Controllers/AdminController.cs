using _12_Weboto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _12_Weboto.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public IActionResult Index()
        {
            var totalCars = _context.Cars.Count();
            var totalBrands = _context.Brands.Count();
            var totalUsers = _context.Users.Count();

            ViewBag.TotalCars = totalCars;
            ViewBag.TotalBrands = totalBrands;
            ViewBag.TotalUsers = totalUsers;

            return View();
        }
    }
}
