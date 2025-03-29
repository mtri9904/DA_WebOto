using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _12_Weboto.Models;
using Microsoft.AspNetCore.Authorization;

namespace _12_Weboto.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class BrandController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BrandController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách thương hiệu
        public async Task<IActionResult> Index()
        {
            var brands = await _context.Brands.ToListAsync();
            return View(brands);
        }

        // Hiển thị chi tiết thương hiệu
        public async Task<IActionResult> Details(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
                return NotFound();
            return View(brand);
        }

        // Hiển thị form thêm thương hiệu
        public IActionResult Add()
        {
            return View();
        }

        // Xử lý thêm thương hiệu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Brand brand)
        {
            try
            {
                _context.Brands.Add(brand);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View(brand);
            }

        }


        // Hiển thị form chỉnh sửa thương hiệu
        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
                return NotFound();
            return View(brand);
        }

        // Xử lý chỉnh sửa thương hiệu
        [HttpPost]
        public async Task<IActionResult> Edit(int id, string TenHang)
        {
            if (id <= 0 || string.IsNullOrEmpty(TenHang))
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }

            try
            {
                brand.TenHang = TenHang;
                _context.Update(brand);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Lỗi khi cập nhật dữ liệu.");
            }
        }


        // Xóa thương hiệu
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
                return NotFound();

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
