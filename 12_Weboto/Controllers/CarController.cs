using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using _12_Weboto.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace _12_Weboto.Controllers
{
    public class CarController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;

        public CarController(IWebHostEnvironment webHostEnvironment, ApplicationDbContext context)
        {
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            try
            {
                // Log để kiểm tra query
                Console.WriteLine($"Search Query in CarController: '{query}'");

                var cars = await _context.Cars
                    .Include(c => c.Images)
                    .Where(c => string.IsNullOrEmpty(query) || c.TenXe.Contains(query))
                    .Select(c => new
                    {
                        tenXe = c.TenXe,
                        giaTien = c.GiaTien,
                        imageUrl = c.Images.FirstOrDefault() != null ? c.Images.FirstOrDefault().ImageUrl : null
                    })
                    .ToListAsync();

                // Log để kiểm tra kết quả
                Console.WriteLine($"Số xe tìm được trong CarController: {cars.Count}");

                return Json(new { success = true, cars });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CarController.Search: {ex.Message}");
                return Json(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var car = await _context.Cars.Include(c => c.Images).Include(c => c.Brand).FirstOrDefaultAsync(c => c.Id == id);
            if (car == null)
            {
                return NotFound();
            }
            return View(car);
        }

        public IActionResult Index()
        {
            var cars = _context.Cars.Include(c => c.Images).Include(c => c.Brand).ToList();
            return View(cars);
        }

        public IActionResult CarList()
        {
            var cars = _context.Cars.Include(c => c.Images).Include(c => c.Brand).ToList();
            return View(cars);
        }
    }
}
