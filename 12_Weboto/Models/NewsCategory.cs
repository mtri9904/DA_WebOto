using System.ComponentModel.DataAnnotations;

namespace _12_Weboto.Models
{
    public class NewsCategory
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public List<News> News { get; set; } = new List<News>();
    }
}
