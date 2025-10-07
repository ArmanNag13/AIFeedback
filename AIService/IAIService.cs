using System.Threading.Tasks;

namespace AIFeedback.Service
{
    public interface IAIService
    {
        Task<string> GetAIResponseAsync(string userMessage);
        Task<string> AnalyzeFeedbackAsync(string messageText);
        Task<List<string>> ReadRecentUnreadEmailsAsync(int maxResults = 5);
    }
}

