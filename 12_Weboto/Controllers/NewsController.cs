using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _12_Weboto.Models;
using System.Threading.Tasks;
using System.Linq;

namespace _12_Weboto.Controllers
{
    public class NewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách tin tức theo danh mục
        public async Task<IActionResult> Index(int? category)
        {
            // Lấy danh sách tất cả danh mục để hiển thị ở trên cùng
            var categories = await _context.NewsCategories
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();
            ViewBag.Categories = categories;

            // Lưu danh mục hiện tại để đánh dấu
            ViewBag.SelectedCategory = category;

            // Lấy danh sách bài viết theo danh mục
            IQueryable<News> newsQuery = _context.News
                .Include(n => n.Category)
                .Include(n => n.Images);

            if (category.HasValue)
            {
                newsQuery = newsQuery.Where(n => n.CategoryId == category);
            }

            var newsList = await newsQuery
                .OrderByDescending(n => n.CreatedAt) // Sắp xếp theo ngày tạo mới nhất
                .Select(n => new News
                {
                    Id = n.Id,
                    Title = n.Title,
                    Content = n.Content.Length > 100 ? n.Content.Substring(0, 100) + "..." : n.Content, // Tóm tắt nội dung
                    CreatedAt = n.CreatedAt,
                    Category = n.Category,
                    Images = n.Images
                })
                .ToListAsync();

            // Lưu tên danh mục hiện tại để hiển thị tiêu đề
            if (category.HasValue)
            {
                var categoryName = await _context.NewsCategories
                    .Where(c => c.Id == category)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync();
                ViewData["CategoryName"] = categoryName;
            }
            else
            {
                ViewData["CategoryName"] = "Danh sách tin tức";
            }

            return View(newsList);
        }

        // Hiển thị chi tiết bài viết
        public async Task<IActionResult> Details(int id)
        {
            // Lấy bài viết hiện tại cùng với danh mục và hình ảnh
            var news = await _context.News
                .Include(n => n.Category)
                .Include(n => n.Images)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (news == null)
            {
                return NotFound();
            }

            // Lấy các bài viết liên quan (cùng danh mục, trừ bài hiện tại)
            var relatedNews = await _context.News
                .Where(n => n.CategoryId == news.CategoryId && n.Id != news.Id)
                .Take(2) // Giới hạn 2 bài
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    ImageUrl = n.Images != null && n.Images.Any() ? n.Images.First().ImageUrl : null
                })
                .ToListAsync();

            // Gán danh sách bài viết liên quan vào ViewBag
            ViewBag.RelatedNews = relatedNews;

            return View(news);
        }


        // GET: News/Create
        public IActionResult Add()
        {
            ViewBag.Categories = _context.NewsCategories.ToList();
            return View();
        }

        // POST: News/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(News news, List<string> imageUrls)
        {
            if (ModelState.IsValid)
            {
                _context.Add(news);
                await _context.SaveChangesAsync();

                if (imageUrls != null && imageUrls.Any())
                {
                    foreach (var imageUrl in imageUrls)
                    {
                        var newsImage = new NewsImage { NewsId = news.Id, ImageUrl = imageUrl };
                        _context.NewsImages.Add(newsImage);
                    }
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = _context.NewsCategories.ToList();
            return View(news);
        }
        public IActionResult GetRandomNews()
        {
            var randomNews = _context.News
                .OrderBy(n => Guid.NewGuid()) // Sắp xếp ngẫu nhiên
                .Take(3) // Lấy 3 tin tức
                .Select(n => new
                {
                    n.Title,
                    n.Id,
                    ImageUrl = n.Images.Any() ? n.Images.FirstOrDefault().ImageUrl : "https://giaxemoto.com.vn/wp-content/uploads/2023/11/hinh-anh-o-to-lamborghini-dep-1709.jpg"
                }) // Lấy ảnh đầu tiên của bài viết, nếu không có thì dùng ảnh mặc định
                .ToList();
            return Json(randomNews);
        }
    }
}
