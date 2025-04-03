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

        [Required]
        public string Address { get; set; }

        public string Notes { get; set; }

        public DateTime OrderDate { get; set; }

        [Required]
        public OrderStatus Status { get; set; }
    }

    public enum OrderStatus
    {
        Processing = 0, // Đang xử lý
        Completed = 1,  // Hoàn thành
        Failed = 2,      // Thất bại
        Pending = 3
    }

}
