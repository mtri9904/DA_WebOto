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
            var cars = _context.Cars.Include(c => c.Images).ToList();
            return View(cars);
        }
        // Hiển thị form chỉnh sửa xe
        public IActionResult Edit(int id)
        {
            var car = _context.Cars.Include(c => c.Images).FirstOrDefault(c => c.Id == id);
            if (car == null)
            {
                return NotFound();
            }
            return View(car);
        }


        // Xử lý cập nhật thông tin xe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Car car, List<IFormFile> NewImages, List<int> DeletedImageIds)
        {
            if (id != car.Id)
            {
                return BadRequest();
            }

            var existingCar = await _context.Cars.Include(c => c.Images).FirstOrDefaultAsync(c => c.Id == id);
            if (existingCar == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin xe
            existingCar.TenXe = car.TenXe;
            existingCar.GiaTien = car.GiaTien;
            existingCar.NamSanXuat = car.NamSanXuat;
            existingCar.NhienLieu = car.NhienLieu;
            existingCar.SoKM = car.SoKM;
            existingCar.SoChoNgoi = car.SoChoNgoi;
            existingCar.HangXe = car.HangXe;
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

            // Xóa ảnh được chọn
            if (DeletedImageIds != null && DeletedImageIds.Count > 0)
            {
                var imagesToDelete = _context.CarImages.Where(img => DeletedImageIds.Contains(img.Id)).ToList();
                foreach (var image in imagesToDelete)
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    _context.CarImages.Remove(image);
                }
            }

            // Thêm ảnh mới
            if (NewImages != null && NewImages.Count > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                foreach (var image in NewImages)
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
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        // Phương thức hiển thị trang xác nhận xóa
        public async Task<IActionResult> Delete(int id)
        {
            var car = await _context.Cars.Include(c => c.Images).FirstOrDefaultAsync(c => c.Id == id);
            if (car == null)
            {
                return NotFound();
            }
            return View(car);
        }

        // Phương thức xử lý xóa xe
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Cars.Include(c => c.Images).FirstOrDefaultAsync(c => c.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            // Xóa ảnh liên quan
            foreach (var image in car.Images)
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                _context.CarImages.Remove(image);
            }

            // Xóa xe khỏi database
            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
