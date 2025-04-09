using _12_Weboto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PayPal.Api; // Chỉ dùng PayPal REST API SDK
using MyOrder = _12_Weboto.Models.Order; // Alias để tránh xung đột với PayPal.Api.Order
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _12_Weboto.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PayPalConfig _payPalConfig;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IOptions<PayPalConfig> payPalConfig)
        {
            _context = context;
            _userManager = userManager;
            _payPalConfig = payPalConfig.Value;
        }

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
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == carId);
            if (car == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Kiểm tra nếu người dùng là admin
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return Forbid(); // Chuyển hướng đến AccessDenied
            }

            var orderId = "DH" + DateTime.Now.Ticks.ToString();
            ViewBag.CarId = carId;
            ViewBag.TenXe = car.TenXe;
            ViewBag.OrderId = orderId;
            ViewBag.FullName = user.FullName;
            ViewBag.TotalPrice = car.GiaTien;
            ViewBag.PhoneNumber = user.PhoneNumber;

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int carId, string Address, string Notes)
        {
            if (string.IsNullOrWhiteSpace(Address))
                return BadRequest("Địa chỉ không được để trống.");

            var car = await _context.Cars.FindAsync(carId);
            if (car == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Kiểm tra nếu người dùng là admin
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return Forbid(); // Chuyển hướng đến AccessDenied
            }

            var orderId = "DH" + Guid.NewGuid().ToString("N");
            var order = new MyOrder
            {
                OrderId = orderId,
                UserId = user.Id,
                CarId = carId,
                TotalPrice = car.GiaTien, // Giá xe để hiển thị
                DepositAmount = 20000000m, // Giá đặt cọc cố định
                PhoneNumber = user.PhoneNumber ?? "N/A",
                Address = Address.Trim(),
                Notes = Notes,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatusEnum.Pending
            };

            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException?.Message ?? ex.Message;
                return BadRequest($"Lỗi khi lưu đơn hàng: {innerException}");
            }

            return RedirectToAction("PayWithPaypal", new { orderId = order.OrderId });
        }

        public async Task<IActionResult> PayWithPaypal(string orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            // Gỡ lỗi: In giá trị của _payPalConfig
            Console.WriteLine($"ClientId: {_payPalConfig.ClientId}");
            Console.WriteLine($"ClientSecret: {_payPalConfig.ClientSecret}");
            Console.WriteLine($"Mode: {_payPalConfig.Mode}");

            if (string.IsNullOrEmpty(_payPalConfig.ClientId) || string.IsNullOrEmpty(_payPalConfig.ClientSecret))
            {
                return BadRequest("Thông tin ClientId hoặc ClientSecret bị thiếu trong cấu hình PayPal.");
            }

            var config = new Dictionary<string, string> { { "mode", _payPalConfig.Mode } };
            var accessToken = new OAuthTokenCredential(_payPalConfig.ClientId, _payPalConfig.ClientSecret, config).GetAccessToken();
            var apiContext = new APIContext(accessToken) { Config = config };

            var payer = new Payer { payment_method = "paypal" };
            var redirectUrls = new RedirectUrls
            {
                cancel_url = Url.Action("PaymentCancel", "Order", new { orderId }, Request.Scheme),
                return_url = Url.Action("PaymentSuccess", "Order", new { orderId }, Request.Scheme)
            };

            // Sử dụng DepositAmount để thanh toán (20,000,000 VND)
            decimal depositAmountVND = order.DepositAmount;
            // Chuyển đổi sang USD (giả sử tỷ giá 1 USD = 25,000 VND)
            decimal depositAmountUSD = depositAmountVND / 25000m;

            var transactionList = new List<Transaction>
    {
        new Transaction
        {
            amount = new Amount { currency = "USD", total = depositAmountUSD.ToString("F2") }, // Thanh toán giá đặt cọc
            description = $"Thanh toán đặt cọc đơn hàng #{order.OrderId}"
        }
    };

            var payment = new Payment
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirectUrls
            };

            try
            {
                var createdPayment = payment.Create(apiContext);
                var approvalUrl = createdPayment.links.First(l => l.rel.ToLower() == "approval_url").href;
                return Redirect(approvalUrl);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi tạo thanh toán PayPal: {ex.Message}");
            }
        }

        public async Task<IActionResult> PaymentSuccess(string orderId, string paymentId, string token, string PayerID)
        {
            Console.WriteLine($"OrderId: {orderId}");
            Console.WriteLine($"PaymentId: {paymentId}");
            Console.WriteLine($"Token: {token}");
            Console.WriteLine($"PayerID: {PayerID}");

            if (string.IsNullOrEmpty(paymentId) || string.IsNullOrEmpty(PayerID))
            {
                Console.WriteLine("Lỗi: PaymentId hoặc PayerID bị thiếu.");
                return View("PaymentFailure");
            }

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                Console.WriteLine("Lỗi: Không tìm thấy đơn hàng.");
                return View("PaymentFailure");
            }

            var config = new Dictionary<string, string> { { "mode", _payPalConfig.Mode } };
            var accessToken = new OAuthTokenCredential(_payPalConfig.ClientId, _payPalConfig.ClientSecret, config).GetAccessToken();
            var apiContext = new APIContext(accessToken) { Config = config };

            var payment = new Payment { id = paymentId };
            try
            {
                var executedPayment = payment.Execute(apiContext, new PaymentExecution { payer_id = PayerID });
                if (executedPayment.state.ToLower() == "approved")
                {
                    order.PaymentStatus = PaymentStatusEnum.Completed; // Chỉ cập nhật trạng thái thanh toán
                    await _context.SaveChangesAsync();
                    TempData["OrderSuccess"] = $"Thanh toán thành công! Mã đơn hàng của bạn là {orderId}.";
                    return RedirectToAction("OrderSuccess");
                }
            }
            catch (PayPal.PaymentsException ex)
            {
                Console.WriteLine($"PayPal Error: {ex.Message}");
                Console.WriteLine($"Response: {ex.Response}");
                return BadRequest($"Lỗi khi thực thi thanh toán PayPal: {ex.Message}");
            }

            return View("PaymentFailure");
        }

        public async Task<IActionResult> PaymentCancel(string orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.PaymentStatus = PaymentStatusEnum.Cancelled; // Chỉ cập nhật trạng thái thanh toán
                await _context.SaveChangesAsync();
            }
            TempData["OrderError"] = $"Thanh toán cho đơn hàng {orderId} đã bị hủy.";
            return RedirectToAction("Add", new { carId = order?.CarId });
        }

        public IActionResult OrderSuccess()
        {
            return View();
        }

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