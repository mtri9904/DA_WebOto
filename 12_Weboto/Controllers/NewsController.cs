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
        // GET: News/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var news = await _context.News
                .Include(n => n.Category)
                .Include(n => n.Images)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (news == null)
            {
                return NotFound();
            }

            // Lấy tin tức liên quan (ví dụ: cùng danh mục, trừ bài hiện tại)
            var relatedNews = await _context.News
                .Where(n => n.CategoryId == news.CategoryId && n.Id != news.Id)
                .OrderByDescending(n => n.CreatedAt)
                .Take(3)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    ImageUrl = n.Images.Any() ? n.Images.First().ImageUrl : null,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            ViewBag.RelatedNews = relatedNews;

            return View(news);
        }
        // Hiển thị danh sách tin tức theo danh mục
        public async Task<IActionResult> Index(int? category)
        {
            var categories = await _context.NewsCategories
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = category;

            IQueryable<News> newsQuery = _context.News
                .Include(n => n.Category)
                .Include(n => n.Images);

            if (category.HasValue)
            {
                newsQuery = newsQuery.Where(n => n.CategoryId == category);
            }

            var newsList = await newsQuery
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new News
                {
                    Id = n.Id,
                    Title = n.Title,
                    Content = n.Content.Length > 100 ? n.Content.Substring(0, 100) + "..." : n.Content,
                    CreatedAt = n.CreatedAt,
                    Category = n.Category,
                    Images = n.Images
                })
                .Take(8) // Chỉ lấy 8 bài đầu tiên
                .ToListAsync();

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

            ViewBag.TotalNews = await newsQuery.CountAsync(); // Tổng số bài viết để kiểm tra "Xem thêm"
            return View(newsList);
        }

        [HttpGet]
        public async Task<IActionResult> LoadMoreNews(int? category, int skip)
        {
            IQueryable<News> newsQuery = _context.News
                .Include(n => n.Category)
                .Include(n => n.Images);

            if (category.HasValue)
            {
                newsQuery = newsQuery.Where(n => n.CategoryId == category);
            }

            // Kiểm tra tổng số bài báo
            var totalNews = await newsQuery.CountAsync();
            if (skip >= totalNews)
            {
                return Json(new List<object>());
            }

            var newsList = await newsQuery
                .Where(n => n.Title != null && n.Content != null) // Lọc các bài báo không có tiêu đề hoặc nội dung
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(4)
                .Select(n => new
                {
                    Id = n.Id,
                    Title = n.Title ?? "Không có tiêu đề",
                    Content = n.Content != null && n.Content.Length > 100 ? n.Content.Substring(0, 100) + "..." : (n.Content ?? "Không có nội dung"),
                    ImageUrl = n.Images != null && n.Images.Any() ? n.Images.First().ImageUrl : "https://via.placeholder.com/200x200?text=No+Image",
                    CategoryName = n.Category != null ? n.Category.Name : "Không có danh mục"
                })
                .ToListAsync();

            return Json(newsList);
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
