using Microsoft.AspNetCore.Mvc;

namespace _12_Weboto.Areas.Admin.Controllers
{
    [Area("Admin")] // Quan trọng
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View(); // Tự động tìm đến Areas/Admin/Views/Admin/Index.cshtml
        }
    }
}
