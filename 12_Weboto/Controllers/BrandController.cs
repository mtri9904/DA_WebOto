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

    public IActionResult Add()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add(Brand brand)
    {
        if (ModelState.IsValid)
        {
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        return View(brand);
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

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var brand = await _context.Brands.FindAsync(id);
        if (brand == null)
        {
            return NotFound();
        }
        return View(brand);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Brand brand)
    {
        if (ModelState.IsValid)
        {
            _context.Brands.Update(brand);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        return View(brand);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var brand = await _context.Brands.FindAsync(id);
        if (brand != null)
        {
            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }
}
