using System.Text.Json.Serialization;

namespace CsharpBot.Models
{  
    public class ChatbotResponse
    {
        public string id { get; set; }
        public string _object { get; set; }
        public int created { get; set; }
        public Choice[] choices { get; set; }
        public Usage usage { get; set; }
    }

    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }

    public class Choice
    {
        public int index { get; set; }
        public Message message { get; set; }
        public string finish_reason { get; set; }
    }

    public class Message
    {
        public string Role { get; set; }  // "user" or "assistant"
        public string Content { get; set; }

        [JsonIgnore]
        public bool IsUser => Role == "user";
    }

}
