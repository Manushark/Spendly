namespace Spendly.Infrastructure.Services.Ai
{
    public class GeminiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gemini-1.5-flash";
        public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
        public int TimeoutSeconds { get; set; } = 15;
    }
}
