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

        // GET: News
        public async Task<IActionResult> Index()
        {
            var news = await _context.News.Include(n => n.Category).Include(n => n.Images).ToListAsync();
            return View(news);
        }
        public async Task<IActionResult> Details(int id)
        {
            var car = await _context.News.Include(c => c.Images).Include(c => c.Category).FirstOrDefaultAsync(c => c.Id == id);
            if (car == null)
            {
                return NotFound();
            }
            return View(car);
        }
        // GET: News/Create
        public IActionResult Add()
        {
            ViewBag.Categories = _context.NewsCategories.ToList();
            return View();
        }

        // POST: News/Create
        [HttpPost]
        public async Task<IActionResult> Add(News news, List<IFormFile> Images)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return View(news);
            }

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
        // GET: News/Edit
        public async Task<IActionResult> Edit(int id)
        {
            var news = await _context.News.Include(n => n.Images).FirstOrDefaultAsync(n => n.Id == id);
            if (news == null)
            {
                return NotFound();
            }

            ViewBag.Categories = _context.NewsCategories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();
            return View(news);
        }

        // POST: News/Edit
        [HttpPost]
        public async Task<IActionResult> Edit(int id, int CategoryId, string Title, string Content, string CreatedBy, List<IFormFile> NewImages, List<int>? DeletedImageIds)
        {
            if (id <= 0 || string.IsNullOrEmpty(Title))
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            var news = await _context.News.FindAsync(id);
            if (news == null)
            {
                return NotFound();
            }
            try
            {
                news.Title = Title;
                news.Content = Content;
                news.CreatedBy = CreatedBy;

                news.CategoryId = CategoryId;

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
                            var newsImage = new NewsImage
                            {
                                NewsId = news.Id,
                                ImageUrl = "/uploads/" + fileName
                            };

                            _context.NewsImages.Add(newsImage);
                        }
                    }
                }
                _context.Update(news);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật tin tức thành công!";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật dữ liệu: " + ex.Message);
                return View(news);
            }
        }

        // GET: News/Delete
        public async Task<IActionResult> Delete(int id)
        {
            var news = await _context.News
                .Include(n => n.Category) // Thêm dòng này
                .Include(n => n.Images)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }

        // POST: News/Delete
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var news = await _context.News.Include(n => n.Images).FirstOrDefaultAsync(n => n.Id == id);
            if (news == null)
            {
                return NotFound();
            }

            // Xóa ảnh khỏi thư mục
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

    }
}
