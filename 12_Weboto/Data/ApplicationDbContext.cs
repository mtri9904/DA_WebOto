using Microsoft.EntityFrameworkCore;
using _12_Weboto.Models;

namespace _12_Weboto.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Car> Cars { get; set; }
        public DbSet<CarImage> CarImages { get; set; }

    }
}
