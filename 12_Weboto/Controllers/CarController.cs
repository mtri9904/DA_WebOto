using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using _12_Weboto.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using _12_Weboto.Data;
using Microsoft.EntityFrameworkCore;
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

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Car car, List<IFormFile> Images)
        {
            if (ModelState.IsValid)
            {
                _context.Cars.Add(car);
                await _context.SaveChangesAsync(); // Lưu để có CarId

                if (Images != null && Images.Count > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    foreach (var image in Images)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        var carImage = new CarImage
                        {
                            CarId = car.Id,
                            ImageUrl = "/uploads/" + uniqueFileName
                        };

                        _context.CarImages.Add(carImage);
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Index");
            }

            return View(car);
        }

        public async Task<IActionResult> Details(int id)
        {
            var car = await _context.Cars.Include(c => c.Images).FirstOrDefaultAsync(c => c.Id == id);
            if (car == null)
            {
                return NotFound();
            }
            return View(car);
        }

        public IActionResult Index()
        {
            var cars = _context.Cars.ToList() ?? new List<Car>(); // Đảm bảo không null
            return View(cars);
        }

    }
}
