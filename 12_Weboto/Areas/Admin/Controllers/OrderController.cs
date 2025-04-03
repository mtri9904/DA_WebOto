using _12_Weboto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _12_Weboto.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Danh sách tất cả đơn hàng
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.Car)
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // Cập nhật trạng thái đơn hàng

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound();

            order.Status = status;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công!";
            return RedirectToAction("Index");
        }
        public IActionResult OrderSuccess()
        {
            return View();
        }

        // Chi tiết đơn hàng
        public async Task<IActionResult> Details(string id)
        {
            var order = await _context.Orders
                .Include(o => o.Car)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            return View(order);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var order = await _context.Orders
                .Include(o => o.Car)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(string orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound();

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa đơn hàng thành công!";
            return RedirectToAction("Index");
        }


    }
}
