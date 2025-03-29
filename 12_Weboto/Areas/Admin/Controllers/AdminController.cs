using _12_Weboto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _12_Weboto.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View(); // Tự động tìm đến Areas/Admin/Views/Admin/Index.cshtml
        }
    }
}
