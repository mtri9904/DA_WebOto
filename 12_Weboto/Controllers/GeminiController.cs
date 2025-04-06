using _12_Weboto.Models;
using _12_Weboto.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;

namespace _12_Weboto.Controllers
{
    public class GeminiController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly ApplicationDbContext _dbContext;
        private readonly IGeminiService _geminiService;
        public GeminiController(IGeminiService geminiService, HttpClient httpClient, IConfiguration configuration, ApplicationDbContext dbContext)
        {
            _geminiService = geminiService;
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"];
            _apiUrl = configuration["Gemini:ApiUrl"];
            _dbContext = dbContext;
        }
        public async Task<IActionResult> Index(string userInput)
        {
            if (!string.IsNullOrEmpty(userInput))
            {
                var aiResponse = await _geminiService.GetAIResponse(userInput);
                ViewBag.AIResponse = aiResponse;
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request?.Message))
            {
                return Json(new { success = false, error = "Vui lòng nhập tin nhắn!" });
            }
            try
            {
                var aiResponse = await GetAIResponse(request.Message);
                return Json(new { success = true, response = aiResponse });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Lỗi: {ex.Message}" });
            }
        }

        public class ChatRequest
        {

            public string Message { get; set; }
         }

        public async Task<string> GetAIResponse(string input)
        {
            try
            {
                // Nếu tin nhắn chứa "tìm xe" hoặc "có bán"
                if (input.ToLower().Contains("tìm xe") || input.ToLower().Contains("có bán"))
                {
                    string searchQuery = input.Replace("tìm xe", "").Replace("có bán", "").Trim();

                    // Log để kiểm tra searchQuery
                    Console.WriteLine($"Search Query: '{searchQuery}'");

                    // Truy vấn từ database
                    var cars = await _dbContext.Cars
                        .Include(c => c.Images)
                        .Where(c => string.IsNullOrEmpty(searchQuery) || c.TenXe.Contains(searchQuery))
                        .Select(c => new
                        {
                            TenXe = c.TenXe,
                            GiaTien = c.GiaTien,
                            ImageUrl = c.Images.FirstOrDefault() != null ? c.Images.FirstOrDefault().ImageUrl : null
                        })
                        .ToListAsync();

                    // Log để kiểm tra kết quả truy vấn
                    Console.WriteLine($"Số xe tìm được: {cars.Count}");

                    if (cars.Any())
                    {
                        return "🔍 Kết quả tìm kiếm:\n" +
                               string.Join("\n", cars.Select(c => $"{c.TenXe} - Giá: {c.GiaTien}đ" +
                               (string.IsNullOrEmpty(c.ImageUrl) ? "" : $"\n<img src='{c.ImageUrl}' width='100' />")));
                    }
                    return "Không có xe nào phù hợp!";
                }

                // Gọi API Gemini nếu không phải tìm kiếm sản phẩm
                var requestBody = new { contents = new[] { new { parts = new[] { new { text = input } } } } };
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var requestUrl = $"{_apiUrl}?key={_apiKey}";
                var response = await _httpClient.PostAsync(requestUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"API call failed: {response.StatusCode} - {errorContent}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(responseString))
                {
                    throw new Exception("Phản hồi từ API Gemini rỗng!");
                }

                var resultAI = JsonSerializer.Deserialize<JsonElement>(responseString);
                string aiResponse = resultAI.GetProperty("candidates")[0]
                                            .GetProperty("content")
                                            .GetProperty("parts")[0]
                                            .GetProperty("text")
                                            .GetString();

                return aiResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return $"Xin lỗi, đã có lỗi xảy ra: {ex.Message}";
            }
        }

    }
}