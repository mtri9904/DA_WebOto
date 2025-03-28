﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _12_Weboto.Models
{
    public class Brand
    {

        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string TenHang { get; set; }  // Tên hãng xe (VD: Toyota, Honda, BMW,...)

        // Danh sách các xe thuộc hãng này
        public List<Car> Cars { get; set; }
    }
}
