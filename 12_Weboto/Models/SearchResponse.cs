using System.Collections.Generic;

namespace _12_Weboto.Models
{
    public class SearchResponse
    {
        public List<CarDto> Cars { get; set; }
    }

    public class CarDto
    {
        public int Id { get; set; }
        public string TenXe { get; set; }
        public decimal GiaTien { get; set; }
        public string Image { get; set; }
    }
}