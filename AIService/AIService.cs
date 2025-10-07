using AIFeedback.Service;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace AIFeedback.Services
{
    public class AIService : IAIService
    {
        private readonly ChatClient _chatClient;

        public AIService()
        {
            string endpoint = "https://i22m-m3in499z-westeurope.openai.azure.com/";
            string key = "BvxxwtmW5qmCcJpyKNNT0NkJjoCofoaEXpFSLwevwJ6d5XKPq1tJJQQJ99AKAC5RqLJXJ3w3AAAAACOGTiEi";
            var credential = new ApiKeyCredential(key);

            var client = new AzureOpenAIClient(new Uri(endpoint), credential);
            _chatClient = client.GetChatClient("gpt-35-turbo");
        }

        public async Task<string> AnalyzeFeedbackAsync(string messageText)
        {
            var response = await _chatClient.CompleteChatAsync(new ChatMessage[]
            {
                new SystemChatMessage(
            "You are an AI assistant that replies naturally and helpfully to any email message. " +
            "For each text, respond with a polite and context-aware reply. " +
            "Do not return JSON — just plain text."
                ),
                new UserChatMessage(messageText)
            });

            return response.Value.Content[0].Text;
        }

        public Task<string> GetAIResponseAsync(string userMessage)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> ReadRecentUnreadEmailsAsync(int maxResults = 5)
        {
            throw new NotImplementedException();
        }
    }
}
