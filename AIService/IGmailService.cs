using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIFeedback.Services.IServices
{
    public interface IGmailService
    {
        Task<List<(string Body, string MessageId)>> ReadRecentUnreadEmailsAsync(int maxResults = 5);
        Task ReplyToEmailAsync(string messageId, string replyText);
    }
}
