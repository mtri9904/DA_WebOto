using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using _12_Weboto.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace _12_Weboto.Controllers
{
    public class CarController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;

        public CarController(IWebHostEnvironment webHostEnvironment, ApplicationDbContext context)
        {
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }
        public async Task<IActionResult> Details(int id)
        {
            var car = await _context.Cars.Include(c => c.Images).Include(c => c.Brand).FirstOrDefaultAsync(c => c.Id == id);
            if (car == null)
            {
                return NotFound();
            }
            return View(car);
        }

        public IActionResult Index()
        {
            var cars = _context.Cars.Include(c => c.Images).Include(c => c.Brand).ToList();
            return View(cars);
        }

        public IActionResult CarList()
        {
            var cars = _context.Cars.Include(c => c.Images).Include(c => c.Brand).ToList();
            return View(cars);
        }
        [HttpGet]
        public async Task<IActionResult> Searchs(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new
                {
                    success = false,
                    message = "Từ khóa tìm kiếm trống",
                    cars = new List<object>()
                });
            }

            var keyword = RemoveDiacritics(query.ToLower().Trim());

            var matchedCars = await _context.Cars
                .Include(c => c.Images)
                .ToListAsync();

            var filteredCars = matchedCars
                .Where(c =>
                    RemoveDiacritics(c.TenXe?.ToLower() ?? "").Contains(keyword) ||
                    RemoveDiacritics(c.MoTa?.ToLower() ?? "").Contains(keyword) ||
                    RemoveDiacritics(c.PhienBan?.ToLower() ?? "").Contains(keyword) ||
                    RemoveDiacritics(c.NhienLieu?.ToLower() ?? "").Contains(keyword)
                )
                .Select(c => new
                {
                    c.Id,
                    TenXe = c.TenXe ?? "Không rõ tên xe",
                    GiaTien = c.GiaTien.ToString("N0") + " VND",
                    ImageUrl = c.Images?.FirstOrDefault()?.ImageUrl ?? "/uploads/default-car.jpg"
                })
                .ToList();

            if (!filteredCars.Any())
            {
                return Json(new
                {
                    success = false,
                    message = "Không tìm thấy xe nào phù hợp với từ khoá của bạn.",
                    cars = filteredCars
                });
            }

            return Json(new
            {
                success = true,
                cars = filteredCars
            });
        }
        // Hàm bỏ dấu tiếng Việt
        public static string RemoveDiacritics(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var normalized = input.Normalize(System.Text.NormalizationForm.FormD);
            var chars = normalized
                .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                .ToArray();

            return new string(chars).Normalize(System.Text.NormalizationForm.FormC);
        }

        [HttpGet]
        public async Task<IActionResult> CompareCarsAsync(string carIds)
        {
            if (string.IsNullOrEmpty(carIds))
            {
                TempData["Error"] = "Bạn cần chọn ít nhất 2 xe để so sánh!";
                return RedirectToAction("CarList");
            }

            var selectedIds = carIds.Split(',')
                .Select(id => int.TryParse(id, out var result) ? result : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();

            // ✅ Include Brand và Images trước khi gọi AsEnumerable
            var cars = _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Images)
                .AsEnumerable() // chuyển sang LINQ in-memory để tránh lỗi SQL
                .Where(c => selectedIds.Contains(c.Id))
                .ToList();

            if (cars.Count < 2)
            {
                TempData["Error"] = "Bạn cần chọn ít nhất 2 xe để so sánh!";
                return RedirectToAction("CarList");
            }

            return View(cars);
        }
        [HttpGet]
        public async Task<IActionResult> CarList(string searchQuery, int page = 1)
        {
            var carsQuery = _context.Cars
                .Include(c => c.Images)
                .Include(c => c.Brand)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                carsQuery = carsQuery.Where(c => c.TenXe.ToLower().Contains(searchQuery) ||
                                                 c.MoTa.ToLower().Contains(searchQuery) ||
                                                 c.Brand.TenHang.ToLower().Contains(searchQuery));
            }

            // Số xe mỗi trang
            int pageSize = 9;
            int totalCars = await carsQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCars / (double)pageSize);

            // Lấy danh sách xe cho trang hiện tại
            var cars = await carsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền thông tin phân trang vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchQuery = searchQuery;

            return View(cars);
        }
        public IActionResult TestDb()
        {
            var test = _context.Cars
                .Take(1)
                .Include(c => c.Brand)
                .ToList();

            return Json(test); // hoặc return View(test);
        }
        [HttpGet]
        public async Task<IActionResult> GetCarsByBrand(int brandId)
        {
            var cars = await _context.Cars
                .Where(c => c.BrandId == brandId)
                .Select(c => new
                {
                    id = c.Id,
                    tenXe = c.TenXe
                })
                .ToListAsync();

            return Json(new { success = true, cars });
        }

        [HttpGet]
        public async Task<IActionResult> GetCarDetails(int id)
        {
            var car = await _context.Cars
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
            {
                return Json(new { success = false });
            }

            return Json(new
            {
                success = true,
                imageUrl = car.Images?.FirstOrDefault()?.ImageUrl ?? "/uploads/default-car.jpg"
            });
        }
        [HttpGet]
        public async Task<IActionResult> CompareCarsSelection()
        {
            var cars = await _context.Cars
                .Include(c => c.Images)
                .Include(c => c.Brand)
                .ToListAsync();
            return View(cars);
        }
    }
}
