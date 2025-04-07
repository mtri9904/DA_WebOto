using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _12_Weboto.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;

namespace _12_Weboto.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class NewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public NewsController(IWebHostEnvironment webHostEnvironment, ApplicationDbContext context)
        {
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var news = await _context.News.Include(n => n.Category).Include(n => n.Images).ToListAsync();
            return View(news);
        }

        public async Task<IActionResult> Details(int id)
        {
            var news = await _context.News.Include(c => c.Images).Include(c => c.Category).FirstOrDefaultAsync(c => c.Id == id);
            if (news == null)
            {
                return NotFound();
            }
            return View(news);
        }

        // GET: News/Add
        public async Task<IActionResult> Add() // Thêm async
        {
            ViewBag.CategoryId = new SelectList(_context.NewsCategories, "Id", "Name");

            // Lấy FullName từ user hiện tại
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            ViewBag.FullName = user != null ? user.FullName : "Anonymous"; // Truyền FullName vào ViewBag

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(News news, List<IFormFile> Images)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
                ViewBag.CategoryId = new SelectList(_context.NewsCategories, "Id", "Name", news.CategoryId);
                return View(news);
            }

            news.CreatedAt = DateTime.Now;
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            news.CreatedBy = user != null ? user.FullName : "Anonymous";

            Console.WriteLine($"Số lượng ảnh nhận được: {Images?.Count ?? 0}"); // Debug số lượng ảnh

            _context.News.Add(news);
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

                    var newsImage = new NewsImage
                    {
                        NewsId = news.Id,
                        ImageUrl = "/uploads/" + uniqueFileName
                    };

                    _context.NewsImages.Add(newsImage);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm xe mới thành công!";
                if (_context.Entry(news).State == EntityState.Detached)
                {
                    Console.WriteLine("Lỗi: Xe không được lưu vào database.");
                }

            }
            ViewBag.Categories = _context.NewsCategories.ToList();
            return RedirectToAction("Index");
        }
        // GET: News/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var news = await _context.News
                .Include(n => n.Images)
                .Include(n => n.Category)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (news == null)
            {
                return NotFound();
            }

            ViewBag.CategoryId = new SelectList(_context.NewsCategories, "Id", "Name", news.CategoryId);
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            ViewBag.FullName = user != null ? user.FullName : "Anonymous"; // Truyền FullName vào ViewBag

            return View(news);
        }

        // POST: News/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, News news, List<IFormFile> Images)
        {
            if (id != news.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
                ViewBag.CategoryId = new SelectList(_context.NewsCategories, "Id", "Name", news.CategoryId);
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                ViewBag.FullName = user != null ? user.FullName : "Anonymous";
                return View(news);
            }

            try
            {
                var existingNews = await _context.News
                    .Include(n => n.Images)
                    .FirstOrDefaultAsync(n => n.Id == id);

                if (existingNews == null)
                {
                    return NotFound();
                }

                // Cập nhật các thuộc tính, giữ nguyên CreatedBy và CreatedAt
                existingNews.Title = news.Title;
                existingNews.Content = news.Content;
                existingNews.CategoryId = news.CategoryId;

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

                        var newsImage = new NewsImage
                        {
                            NewsId = existingNews.Id,
                            ImageUrl = "/uploads/" + uniqueFileName
                        };

                        _context.NewsImages.Add(newsImage);
                    }
                }

                _context.Update(existingNews);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật tin tức thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NewsExists(news.Id))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        private bool NewsExists(int id)
        {
            return _context.News.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var news = await _context.News
                .Include(n => n.Category)
                .Include(n => n.Images)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var news = await _context.News.Include(n => n.Images).FirstOrDefaultAsync(n => n.Id == id);
            if (news == null)
            {
                return NotFound();
            }

            if (news.Images != null)
            {
                foreach (var img in news.Images)
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, img.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }

            _context.News.Remove(news);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa tin tức thành công!";
            return RedirectToAction("Index");
        }
   
    [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var imageUrl = "/uploads/" + uniqueFileName;
                return Json(new { location = imageUrl });
            }

            return BadRequest("No file uploaded");
        }
    }
}