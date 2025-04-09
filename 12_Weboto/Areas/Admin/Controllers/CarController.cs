    using Microsoft.AspNetCore.Mvc;
    using _12_Weboto.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Authorization;

    namespace _12_Weboto.Areas.Admin.Controllers
    {
        [Area("Admin")]
        [Authorize(Roles = SD.Role_Admin)]
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
                // Ghi log thông tin xe để debug
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
                        // Tạo đối tượng CarImage và liên kết với xe vừa thêm
                        var carImage = new CarImage
                        {
                            CarId = car.Id,
                            ImageUrl = "/uploads/" + uniqueFileName
                        };

                        _context.CarImages.Add(carImage);
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Thêm xe mới thành công!";
                    // Kiểm tra trạng thái của xe trong context (debug)
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
                // Lấy thông tin xe kèm ảnh và hãng xe
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
            var car = await _context.Cars
                .Include(c => c.Images)
                .Include(c => c.Brand)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (car == null)
            {
                return NotFound();
            }
            ViewBag.BrandId = new SelectList(_context.Brands, "Id", "TenHang", car.BrandId);
            return View(car);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Car car, List<IFormFile>? NewImages, List<int>? DeletedImageIds)
        {
            if (id != car.Id)
            {
                return NotFound();
            }

            // Xóa validation của Images khỏi ModelState (vì nó không được gửi từ form)
            ModelState.Remove("Images");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
                ViewBag.BrandId = new SelectList(_context.Brands, "Id", "TenHang", car.BrandId);
                return View(car);
            }

            try
            {
                var existingCar = await _context.Cars
                    .Include(c => c.Images)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (existingCar == null)
                {
                    return NotFound();
                }

                // Cập nhật các thuộc tính của xe
                existingCar.TenXe = car.TenXe;
                existingCar.GiaTien = car.GiaTien;
                existingCar.NamSanXuat = car.NamSanXuat;
                existingCar.NhienLieu = car.NhienLieu;
                existingCar.SoKM = car.SoKM;
                existingCar.SoChoNgoi = car.SoChoNgoi;
                existingCar.BrandId = car.BrandId;
                existingCar.PhienBan = car.PhienBan;
                existingCar.KieuDang = car.KieuDang;
                existingCar.XuatXu = car.XuatXu;
                existingCar.DongXe = car.DongXe;
                existingCar.DongCo = car.DongCo;
                existingCar.HopSo = car.HopSo;
                existingCar.ChieuDai = car.ChieuDai;
                existingCar.ChieuRong = car.ChieuRong;
                existingCar.ChieuCao = car.ChieuCao;
                existingCar.CoSoBanhXe = car.CoSoBanhXe;
                existingCar.TrongLuongKhongTai = car.TrongLuongKhongTai;
                existingCar.LopTruoc = car.LopTruoc;
                existingCar.LopSau = car.LopSau;
                existingCar.MucTieuThuNgoaiDoThi = car.MucTieuThuNgoaiDoThi;
                existingCar.MucTieuThuTrongDoThi = car.MucTieuThuTrongDoThi;
                existingCar.MoTa = car.MoTa;

                // Xử lý xóa ảnh cũ (nếu có)
                if (DeletedImageIds != null && DeletedImageIds.Count > 0)
                {
                    var imagesToDelete = existingCar.Images.Where(img => DeletedImageIds.Contains(img.Id)).ToList();
                    foreach (var img in imagesToDelete)
                    {
                        // Xóa file ảnh từ thư mục uploads
                        string filePath = Path.Combine(_webHostEnvironment.WebRootPath, img.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                        // Xóa ảnh khỏi danh sách và cơ sở dữ liệu
                        existingCar.Images.Remove(img);
                        _context.CarImages.Remove(img);
                    }
                }

                // Thêm ảnh mới (nếu có)
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

                            var newImage = new CarImage
                            {
                                CarId = existingCar.Id,
                                ImageUrl = "/uploads/" + fileName
                            };
                            existingCar.Images.Add(newImage);
                            _context.CarImages.Add(newImage);
                        }
                    }
                }

                // Cập nhật xe vào database
                _context.Update(existingCar);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật xe thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                // Xử lý lỗi đồng thời khi cập nhật
                if (!_context.Cars.Any(e => e.Id == car.Id))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction("Index");
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
                TempData["Success"] = "Xóa xe thành công!";
                return RedirectToAction("Index");
            }
        }
    }
