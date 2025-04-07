using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using _12_Weboto.Models;
using _12_Weboto.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _apiUrl;
    private readonly ApplicationDbContext _dbContext;
    public GeminiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient; _apiKey = configuration["Gemini:ApiKey"]; _apiUrl = configuration["Gemini:ApiUrl"];
    }
    public async Task<string> GetAIResponse(string input)
    {
        try
        {
            // Nếu tin nhắn chứa "tìm xe", gọi API tìm kiếm xe
            if (input.ToLower().Contains("tìm xe") || input.ToLower().Contains("có bán"))
            {
                string searchQuery = input.Replace("tìm xe", "").Trim();
                var searchResponse = await _httpClient.GetAsync($"https://localhost:5001/Car/Search?query={searchQuery}");

                if (!searchResponse.IsSuccessStatusCode)
                    return "Không tìm thấy xe nào phù hợp.";

                var responseBody = await searchResponse.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<SearchResponse>(responseBody);

                if (result.Cars.Any())
                {
                    return "🔍 Kết quả tìm kiếm:\n" +
                           string.Join("\n", result.Cars.Select(c => $"{c.TenXe} - Giá: {c.GiaTien}đ"));
                }
                return "Không có xe phù hợp!";
            }

            // Gọi API Gemini nếu không phải tìm kiếm sản phẩm
            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = input } } } }
            };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var requestUrl = $"{_apiUrl}?key={_apiKey}";
            var response = await _httpClient.PostAsync(requestUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API call failed: {response.StatusCode} - {errorContent}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
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
            return "Xin lỗi, đã có lỗi xảy ra!";
        }
    }


}