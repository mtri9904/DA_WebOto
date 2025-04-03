using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _12_Weboto.Models;
using System.Linq;
using System.Threading.Tasks;

public class BrandController : Controller
{
    private readonly ApplicationDbContext _context;

    public BrandController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var brands = _context.Brands.Include(b => b.Cars).ToList();
        return View(brands);
    }

    public async Task<IActionResult> Details(int id)
    {
        var brand = await _context.Brands.Include(b => b.Cars)
                                         .FirstOrDefaultAsync(b => b.Id == id);
        if (brand == null)
        {
            return NotFound();
        }
        return View(brand);
    }
}
