namespace _12_Weboto.Service
{
    public interface IGeminiService
    {
        Task<string> GetAIResponse(string input);
    }
}