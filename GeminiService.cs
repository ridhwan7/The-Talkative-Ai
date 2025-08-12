using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CsharpBot.Models;

namespace CsharpBot.Services
{

    public static class TextHighlighter
    {
        public static string BoldKeywords(string text, string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                // Escape special regex characters in keyword
                var escaped = System.Text.RegularExpressions.Regex.Escape(keyword);
                text = System.Text.RegularExpressions.Regex.Replace(
                    text,
                    $@"\b({escaped})\b", // Match whole words
                    "<strong>$1</strong>",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
            }
            return text;
        }
    }

    public class GeminiService
    {
        private readonly HttpClient _http;
        private string _apiKey;
        private string _apiUrl;

        public GeminiService(HttpClient http)
        {
            _http = http;
        }

        public async Task InitAsync()
        {
            // Load config from wwwroot/appsettings.json
            var configStream = await _http.GetStreamAsync("appsettings.json");
            using var doc = await JsonDocument.ParseAsync(configStream);
            var gemini = doc.RootElement.GetProperty("Gemini");

            _apiKey = gemini.GetProperty("ApiKey").GetString();
            _apiUrl = gemini.GetProperty("ApiUrl").GetString();
        }

        public async Task<string> GetChatCompletionAsync(List<Message> messages)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_apiUrl))
                throw new InvalidOperationException("GeminiService not initialized. Call InitAsync() first.");

            var url = $"{_apiUrl}?key={_apiKey}";

            var contents = messages
                .Where(m => m.Role != "system") // Gemini doesn't support system role
                .Select(m => new
                {
                    role = m.Role.ToLower(),
                    parts = new[] { new { text = m.Content } }
                })
                .ToList();

            var requestBody = new { contents };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _http.PostAsync(url, content);
                var resultJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Gemini API error: {resultJson}");
                    return $"Error: {response.StatusCode} - {resultJson}";
                }

                using var doc = JsonDocument.Parse(resultJson);
                return doc.RootElement
                          .GetProperty("candidates")[0]
                          .GetProperty("content")
                          .GetProperty("parts")[0]
                          .GetProperty("text")
                          .GetString();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Exception calling Gemini API: {ex}");
                return $"Exception: {ex.Message}";
            }
        }
    }
}
