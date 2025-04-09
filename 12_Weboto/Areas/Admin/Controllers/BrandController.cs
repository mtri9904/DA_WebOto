using Microsoft.AspNetCore.Mvc;
using _12_Weboto.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace _12_Weboto.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class BrandController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;
        public BrandController(IWebHostEnvironment webHostEnvironment, ApplicationDbContext context)
        {
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Thêm để bảo vệ chống tấn công CSRF
        public async Task<IActionResult> Add(Brand brand, IFormFile Image)
        {
            // Lấy tất cả lỗi validation
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                foreach (var error in errors)
                {
                    Console.WriteLine(error); // Ghi log ra console
                    TempData["Error"] = error; // Lưu lỗi vào TempData để hiển thị trên view
                }
                return View(brand);
            }

            try
            {
                if (Image != null && Image.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    // Kiểm tra và tạo thư mục uploads
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    // Tạo tên file duy nhất
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                    // Định nghĩa đường dẫn đầy đủ để lưu file
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    // Lưu file ảnh vào thư mục uploads
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        // Sao chép nội dung file từ client lên server
                        await Image.CopyToAsync(stream);
                    }

                    brand.ImageUrl = "/uploads/" + uniqueFileName;
                }

                _context.Brands.Add(brand);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm thương hiệu thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi lưu: {ex.Message}";
                return View(brand);
            }

            return RedirectToAction("Index");
        }

        // Hiển thị danh sách thương hiệu
        public async Task<IActionResult> Index()
        {
            var brands = await _context.Brands.ToListAsync();
            return View(brands);
        }
        public async Task<IActionResult> Details(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy
            }
            return View(brand);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Brand brand, IFormFile? Image)
        {
            if (id != brand.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(brand);
            }

            var existingBrand = await _context.Brands.FindAsync(id);
            if (existingBrand == null)
            {
                return NotFound();
            }

            existingBrand.TenHang = brand.TenHang;

            if (Image != null && Image.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Image.CopyToAsync(stream);
                }

                existingBrand.ImageUrl = "/uploads/" + uniqueFileName;
            }

            _context.Brands.Update(existingBrand);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật thương hiệu thành công!";

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            // Tìm thương hiệu theo ID
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }
        [HttpPost]
        // Xóa thương hiệu
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
                return NotFound();

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa thương hiệu thành công!";
            return RedirectToAction("Index");
        }
    }
}
