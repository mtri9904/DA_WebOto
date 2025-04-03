using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace _12_Weboto.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Car> Cars { get; set; }

        public DbSet<Brand> Brands { get; set; }
        public DbSet<CarImage> CarImages { get; set; }

        public DbSet<NewsCategory> NewsCategories { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<NewsImage> NewsImages { get; set; }

        public DbSet<Order> Orders { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Thêm dữ liệu mặc định cho danh mục tin tức
            modelBuilder.Entity<NewsCategory>().HasData(
                new NewsCategory { Id = 1, Name = "Tin Trong Nước" },
                new NewsCategory { Id = 2, Name = "Tin Nước Ngoài" },
                new NewsCategory { Id = 3, Name = "Tin Xe Mới" },
                new NewsCategory { Id = 4, Name = "Tin Khuyến Mãi" }
            );
        }
    }
}
