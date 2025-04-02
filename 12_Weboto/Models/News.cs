using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace _12_Weboto.Models
{
    public class News
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }  // Tiêu đề chính

        [Required]
        public string Content { get; set; }  // Nội dung bài viết (có thể chứa ảnh nhúng)

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string CreatedBy { get; set; }

        public int CategoryId { get; set; }

        public NewsCategory? Category { get; set; }

        public ICollection<NewsImage> Images { get; set; }  // Danh sách ảnh của bài viết
    }
    public class NewsImage
    {
        [Key]
        public int Id { get; set; }

        public int NewsId { get; set; }

        public News News { get; set; }

        public string ImageUrl { get; set; }  // Đường dẫn ảnh
    }
}