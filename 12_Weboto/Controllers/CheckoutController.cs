using _12_Weboto.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PayPal.Manager;
using PayPal;

namespace _12_Weboto.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PayPalConfig _payPalConfig;

        public CheckoutController(ApplicationDbContext context, IOptions<PayPalConfig> payPalConfig)
        {
            _context = context;
            _payPalConfig = payPalConfig.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ConfirmOrder(string customerName, string address, string email, string city, string state, string zipCode, decimal amount)
        {
            var order = new Order
            {
                CustomerName = customerName,
                Address = address,
                Email = email,
                TotalAmount = amount,
                PaymentStatus = "Pending"
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            return RedirectToAction("PayWithPaypal", new { orderId = order.OrderId });
        }


        public IActionResult PayWithPaypal(int orderId)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null) return NotFound();

            var config = new Dictionary<string, string> { { "mode", _payPalConfig.Mode } };
            var accessToken = new OAuthTokenCredential(_payPalConfig.ClientId, _payPalConfig.ClientSecret, config).GetAccessToken();
            var apiContext = new APIContext(accessToken) { Config = config };


            var payer = new Payer() { payment_method = "paypal" };
            var redirectUrls = new RedirectUrls()
            {
                cancel_url = Url.Action("Cancel", "Checkout", null, Request.Scheme),
                return_url = Url.Action("Success", "Checkout", new { orderId }, Request.Scheme)
            };

            var transactionList = new List<Transaction>
            {
                new Transaction()
                {
                amount = new Amount() { currency = "USD", total = order.TotalAmount.ToString("F2") },
                description = $"Thanh toán đơn hàng #{order.OrderId}"
            }
        };

            var payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirectUrls
            };

            var createdPayment = payment.Create(apiContext);
            var approvalUrl = createdPayment.links.First(l => l.rel.ToLower() == "approval_url").href;

            return Redirect(approvalUrl);
        }

        public IActionResult Success(int orderId, string paymentId, string token, string PayerID)
        {
            if (string.IsNullOrEmpty(paymentId) || string.IsNullOrEmpty(PayerID))
            {
                return View("Failure");
            }

            var order = _context.Orders.Find(orderId);
            if (order == null) return View("Failure");

            var config = new Dictionary<string, string> { { "mode", _payPalConfig.Mode } };
            var accessToken = new OAuthTokenCredential(_payPalConfig.ClientId, _payPalConfig.ClientSecret, config).GetAccessToken();
            var apiContext = new APIContext(accessToken) { Config = config };

            var payment = new Payment() { id = paymentId };
            var executedPayment = payment.Execute(apiContext, new PaymentExecution() { payer_id = PayerID });

            if (executedPayment.state.ToLower() == "approved")
            {
                order.PaymentStatus = "Completed";
                _context.SaveChanges();
                return View("Success");
            }

            return View("Failure");
        }

        public IActionResult Cancel()
        {
            return View();
        }
    }

}
