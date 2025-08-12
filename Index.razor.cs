using CsharpBot.Models;
using CsharpBot.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TalkativeAi.Pages
{
    public partial class Index : ComponentBase
    {
        // Inject GeminiService from DI container
        [Inject]
        public GeminiService GeminiService { get; set; }
        private Message _currentProcessingMessage;

        private string _userQuestion = string.Empty;
        private readonly List<Message> _conversationHistory = new();
        private bool _isSendingMessage;
        private bool isreplying;
        private readonly string _chatBotKnowledgeScope =
            "Your name is CsharpBot, you are an assistant that helps users learn C#." +
            " When the user's question is not related to C# or the .NET framework, reply politely that you cannot answer." +
            " Format every response in HTML.";

        protected override async Task OnInitializedAsync()
        {
            await GeminiService.InitAsync();

            _conversationHistory.Add(new Message
            {
                Role = "system",
                Content = _chatBotKnowledgeScope
            });
        }

        private async Task HandleKeyPress(KeyboardEventArgs e)
        {
            if (e.Key is not "Enter") return;
            await SendMessage();
        }

        private async Task SendMessage()
        {
            var userMsg = new Message
            {
                Role = "user",
                Content = _userQuestion
            };

            _conversationHistory.Add(userMsg);
            _currentProcessingMessage = userMsg; // Track which message is being processed
            _isSendingMessage = true;

            StateHasChanged();

            await CreateCompletion();

            _isSendingMessage = false;
            _currentProcessingMessage = null; // Done processing
            ClearInput();

            StateHasChanged();

        }

        private void ClearInput() => _userQuestion = string.Empty;

        private void ClearConversation()
        {
            ClearInput();
            _conversationHistory.Clear();
        }

        private async Task CreateCompletion()
        {
            _isSendingMessage = true;

            // Call Gemini with the entire conversation history
            var assistantText = await GeminiService.GetChatCompletionAsync(_conversationHistory);

            // Add Gemini's reply to the conversation
            _conversationHistory.Add(new Message
            {
                Role = "assistant",
                Content = assistantText
            });

            _isSendingMessage = false;
        }

        private void AddUserQuestionToConversation()
        {
            _conversationHistory.Add(new Message
            {
                Role = "user",
                Content = _userQuestion
            });
        }

        // Expose conversation history without the system message
        public List<Message> Messages =>
            _conversationHistory.Where(c => c.Role is not "system").ToList();
    }
}
