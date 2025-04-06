using System.Collections.Generic;

namespace _12_Weboto.Models
{
    public class SearchResponse
    {
        public bool Success { get; set; }
        public List<Car> Cars { get; set; }
    }
}
