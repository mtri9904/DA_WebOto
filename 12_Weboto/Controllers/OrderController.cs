using _12_Weboto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace _12_Weboto.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Danh sách đơn hàng của người dùng
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var orders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .Include(o => o.Car)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
        [HttpGet]
        public async Task<IActionResult> Add(int carId)
        {
            // Lấy thông tin xe từ carId
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == carId);
            if (car == null)
            {
                return NotFound();
            }

            // Lấy thông tin người dùng hiện tại
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Tạo OrderId (hoặc có thể lấy từ cơ sở dữ liệu nếu cần)
            var orderId = "DH" + DateTime.Now.Ticks.ToString();

            // Truyền các thông tin cần thiết vào ViewBag
            ViewBag.CarId = carId;
            ViewBag.TenXe = car.TenXe;
            ViewBag.OrderId = orderId; // OrderId
            ViewBag.FullName = user.FullName; // FullName
            ViewBag.TotalPrice = car.GiaTien; // TotalPrice
            ViewBag.PhoneNumber = user.PhoneNumber; // TotalPrice
            
            return View();

        }


        [HttpPost]
        public async Task<IActionResult> Add(int carId, string Address,int PhoneNumber, string Notes)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Address))
                {
                    return BadRequest("Địa chỉ không được để trống.");
                }

                // Lấy thông tin xe
                var car = await _context.Cars.FindAsync(carId);
                if (car == null)
                {
                    return NotFound();
                }

                // Lấy thông tin người dùng hiện tại
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Tạo OrderId
                var orderId = "DH" + DateTime.Now.Ticks.ToString();

                // Tạo đơn hàng mới
                var order = new Order
                {
                    OrderId = orderId,
                    UserId = user.Id,
                    CarId = carId,
                    TotalPrice = car.GiaTien,
                    PhoneNumber = user.PhoneNumber,
                    Address = Address.Trim(),
                    Notes = Notes,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.Processing
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                TempData["OrderSuccess"] = $"Đặt hàng thành công! Mã đơn hàng của bạn là {orderId}. Đơn hàng đang được xử lý.";

                return RedirectToAction("OrderSuccess");
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException?.Message ?? ex.Message;
                return BadRequest($"Lỗi khi lưu đơn hàng: {innerException}");
            }
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
    }
}
