using _12_Weboto.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
public class GeminiController : Controller
{
    private readonly IGeminiService _geminiService;
    public GeminiController(IGeminiService geminiService)
    {
        _geminiService = geminiService;
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
    public async Task<IActionResult> Chat([FromBody] ChatRequest
request)
    {
        if (string.IsNullOrEmpty(request?.Message))
        {
            return Json(new
            {
                success = false,
                error = "Vui lòng nhập tin nhắn!" });
            }
        try
            {
                var aiResponse = await _geminiService.GetAIResponse(request.Message);

                return Json(new { success = true, response = aiResponse });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
    public class ChatRequest
    {

        public string Message { get; set; }
    }
