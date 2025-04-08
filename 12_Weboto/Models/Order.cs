using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _12_Weboto.Models
{
    public class Order
    {
        [Key]
        public string OrderId { get; set; }

        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser User { get; set; }

        [Required]
        public int CarId { get; set; }
        public Car Car { get; set; } // Navigation property

        public decimal TotalPrice { get; set; }

        public decimal DepositAmount { get; set; } // Giá đặt cọc

        [Required]
        public string Address { get; set; }

        public string PhoneNumber { get; set; }
        public string Notes { get; set; }

        public DateTime OrderDate { get; set; }

        [Required]
        public OrderStatus Status { get; set; }

        [Required]
        public PaymentStatusEnum PaymentStatus { get; set; } // Trạng thái thanh toán riêng
    }

    public enum OrderStatus
    {
        Processing = 0, // Đang xử lý
        Completed = 1,  // Hoàn thành
        Failed = 2,      // Thất bại
        Pending = 3,
        Cancelled = 4
    }
    // Trạng thái thanh toán (riêng cho quá trình thanh toán)
    public enum PaymentStatusEnum
    {
        Pending = 0,    // Chưa thanh toán
        Completed = 1,  // Đã thanh toán thành công
        Failed = 2,     // Thanh toán thất bại
        Cancelled = 3   // Thanh toán bị hủy
    }
}
