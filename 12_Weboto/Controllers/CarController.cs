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
using System.Globalization;
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

        public IActionResult Add()
        {
            ViewBag.HangXeList = new SelectList(_context.Brands, "Id", "TenHang");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Car car, List<IFormFile> Images)
        {
            Console.WriteLine($"TenXe: {car.TenXe}, GiaTien: {car.GiaTien}, BrandId: {car.BrandId}");
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return View(car);
            }

            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            if (Images != null && Images.Count > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                foreach (var image in Images)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    var carImage = new CarImage
                    {
                        CarId = car.Id,
                        ImageUrl = "/uploads/" + uniqueFileName
                    };

                    _context.CarImages.Add(carImage);
                }

                await _context.SaveChangesAsync();
                if (_context.Entry(car).State == EntityState.Detached)
                {
                    Console.WriteLine("Lỗi: Xe không được lưu vào database.");
                }

            }
            ViewBag.HangXeList = new SelectList(_context.Brands, "Id", "TenHang");
            return RedirectToAction("Index");
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
        public async Task<IActionResult> Edit(int id)
        {
            var car = await _context.Cars.Include(c => c.Images).Include(c => c.Brand).FirstOrDefaultAsync(c => c.Id == id);
            if (car == null)
            {
                return NotFound();
            }
            ViewBag.HangXeList = new SelectList(_context.Brands, "Id", "TenHang");
            return View(car);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, string TenXe, decimal GiaTien, int NamSanXuat, string NhienLieu, int SoKM, int SoChoNgoi,
            int BrandId, string PhienBan, string KieuDang, string XuatXu, string DongXe,
            string DongCo, string HopSo,
            int ChieuDai, int ChieuRong, int ChieuCao, int CoSoBanhXe, int TrongLuongKhongTai,
            string LopTruoc, string LopSau, float MucTieuThuNgoaiDoThi, float MucTieuThuTrongDoThi,
            string MoTa, List<IFormFile> NewImages, List<int>? DeletedImageIds)
        {
            if (id <= 0 || string.IsNullOrEmpty(TenXe))
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            try
            {
                car.TenXe = TenXe;
                car.GiaTien = GiaTien;
                car.NamSanXuat = NamSanXuat;
                car.NhienLieu = NhienLieu;
                car.SoKM = SoKM;
                car.SoChoNgoi = SoChoNgoi;
                car.BrandId = BrandId;
                car.PhienBan = PhienBan;
                car.KieuDang = KieuDang;
                car.XuatXu = XuatXu;
                car.DongXe = DongXe;
                car.DongCo = DongCo;
                car.HopSo = HopSo;
                car.ChieuDai = ChieuDai;
                car.ChieuRong = ChieuRong;
                car.ChieuCao = ChieuCao;
                car.CoSoBanhXe = CoSoBanhXe;
                car.TrongLuongKhongTai = TrongLuongKhongTai;
                car.LopTruoc = LopTruoc;
                car.LopSau = LopSau;
                car.MucTieuThuNgoaiDoThi = MucTieuThuNgoaiDoThi;
                car.MucTieuThuTrongDoThi = MucTieuThuTrongDoThi;
                car.MoTa = MoTa;

                if (DeletedImageIds != null && DeletedImageIds.Count > 0)
                {
                    var imagesToDelete = _context.CarImages.Where(img => DeletedImageIds.Contains(img.Id)).ToList();
                    foreach (var img in imagesToDelete)
                    {
                        string filePath = Path.Combine(_webHostEnvironment.WebRootPath, img.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                        _context.CarImages.Remove(img);
                    }
                }

                // **2. THÊM ẢNH MỚI**
                if (NewImages != null && NewImages.Count > 0)
                {
                    var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    foreach (var file in NewImages)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            var filePath = Path.Combine(uploadPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            // Lưu đường dẫn ảnh vào database
                            var newImage = new CarImage
                            {
                                CarId = car.Id,
                                ImageUrl = "/uploads/" + fileName
                            };

                            _context.CarImages.Add(newImage);
                        }
                    }
                }
                _context.Update(car);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật dữ liệu: " + ex.Message);
                return View(car);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var car = await _context.Cars.Include(c => c.Brand).FirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Cars.FindAsync(id);

            if (car == null)
            {
                return NotFound();
            }

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Search(string query)
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

        // CarController.cs
        [HttpGet]
        public async Task<IActionResult> CarList(string searchQuery)
        {
            var cars = from c in _context.Cars.Include(c => c.Images).Include(c => c.Brand)
                       select c;

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                cars = cars.Where(c => c.TenXe.ToLower().Contains(searchQuery) ||
                                       c.MoTa.ToLower().Contains(searchQuery) ||
                                       c.Brand.TenHang.ToLower().Contains(searchQuery));
            }

            return View(await cars.ToListAsync());
        }




        public IActionResult TestDb()
        {
            var test = _context.Cars
                .Take(1)
                .Include(c => c.Brand)
                .ToList();

            return Json(test); // hoặc return View(test);
        }

    }
}

