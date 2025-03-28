using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using _12_Weboto.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
            ViewBag.Brands = _context.Brands.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Car car, List<IFormFile> Images)
        {
            if (ModelState.IsValid)
            {
                _context.Cars.Add(car);
                await _context.SaveChangesAsync();

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
            ViewBag.Brands = _context.Brands.ToList();
            return View(car);
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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var car = await _context.Cars.Include(c => c.Images).Include(c => c.Brand).FirstOrDefaultAsync(c => c.Id == id);
            if (car == null)
            {
                return NotFound();
            }
            ViewBag.Brands = _context.Brands.ToList();
            return View(car);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Car car, List<IFormFile> NewImages, List<int>? DeletedImageIds)
        {
            if (ModelState.IsValid)
            {
                var existingCar = await _context.Cars.Include(c => c.Images).FirstOrDefaultAsync(c => c.Id == car.Id);

                if (existingCar == null)
                {
                    return NotFound();
                }

                existingCar.TenXe = car.TenXe;
                existingCar.GiaTien = car.GiaTien;
                existingCar.NamSanXuat = car.NamSanXuat;
                existingCar.NhienLieu = car.NhienLieu;
                existingCar.SoKM = car.SoKM;
                existingCar.SoChoNgoi = car.SoChoNgoi;
                existingCar.BrandId = car.BrandId;
                existingCar.PhienBan = car.PhienBan;
                existingCar.KieuDang = car.KieuDang;
                existingCar.XuatXu = car.XuatXu;
                existingCar.DongXe = car.DongXe;
                existingCar.DongCo = car.DongCo;
                existingCar.HopSo = car.HopSo;
                existingCar.ChieuDai = car.ChieuDai;
                existingCar.ChieuRong = car.ChieuRong;
                existingCar.ChieuCao = car.ChieuCao;
                existingCar.CoSoBanhXe = car.CoSoBanhXe;
                existingCar.TrongLuongKhongTai = car.TrongLuongKhongTai;
                existingCar.LopTruoc = car.LopTruoc;
                existingCar.LopSau = car.LopSau;
                existingCar.MucTieuThuNgoaiDoThi = car.MucTieuThuNgoaiDoThi;
                existingCar.MucTieuThuTrongDoThi = car.MucTieuThuTrongDoThi;
                existingCar.MoTa = car.MoTa;

                if (DeletedImageIds != null && DeletedImageIds.Count > 0)
                {
                    var imagesToDelete = _context.CarImages.Where(img => DeletedImageIds.Contains(img.Id)).ToList();
                    foreach (var img in imagesToDelete)
                    {
                        string filePath = Path.Combine(_webHostEnvironment.WebRootPath, img.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                        _context.CarImages.Remove(img);
                    }
                }

                if (NewImages != null && NewImages.Count > 0)
                {
                    var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    foreach (var file in NewImages)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            var filePath = Path.Combine(uploadPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            var newImage = new CarImage
                            {
                                CarId = car.Id,
                                ImageUrl = "/uploads/" + fileName
                            };

                            _context.CarImages.Add(newImage);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.Brands = _context.Brands.ToList();
            return View(car);
        }
    }
}
