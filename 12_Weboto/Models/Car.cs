using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _12_Weboto.Models
{
    public class Car
    {
        [Key]
        public int Id { get; set; }

        // Thông tin cơ bản
        [Required]
        public string TenXe { get; set; }

        public decimal GiaTien { get; set; }
        public int NamSanXuat { get; set; }
        public string NhienLieu { get; set; }
        public int SoKM { get; set; }
        public int SoChoNgoi { get; set; }

        // Tổng quan
        public string HangXe { get; set; }
        public string PhienBan { get; set; }
        public string KieuDang { get; set; }
        public string XuatXu { get; set; }
        public string DongXe { get; set; }

        // Động cơ
        public string DongCo { get; set; }
        public string HopSo { get; set; }

        // Kích thước trọng lượng
        public int ChieuDai { get; set; }
        public int ChieuRong { get; set; }
        public int ChieuCao { get; set; }
        public int CoSoBanhXe { get; set; }
        public int TrongLuongKhongTai { get; set; }

        // Mâm & Vành xe
        public string LopTruoc { get; set; }
        public string LopSau { get; set; }
        public float MucTieuThuNgoaiDoThi { get; set; }
        public float MucTieuThuTrongDoThi { get; set; }

        // Mô tả
        public string MoTa { get; set; }

        // Danh sách ảnh liên kết với xe
        public List<CarImage> Images { get; set; } = new List<CarImage>();
    }

        public class CarImage
        {
            [Key]
            public int Id { get; set; }
            public string ImageUrl { get; set; }

            [ForeignKey("Car")]
            public int CarId { get; set; }
            public Car Car { get; set; }
        }
    
}