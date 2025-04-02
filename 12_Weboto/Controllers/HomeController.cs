using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using _12_Weboto.Models;
using Microsoft.EntityFrameworkCore;

namespace _12_Weboto.Controllers;

public class HomeController : Controller
{

    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        var cars = _context.Cars.Include(c => c.Images).ToList(); // Load danh sách xe và hình ?nh
        return View(cars); // ??m b?o Model không null
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    public IActionResult About()
    {
        return View();
    }
}
