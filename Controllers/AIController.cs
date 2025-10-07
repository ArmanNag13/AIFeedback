using AIFeedback.Service;
using Microsoft.AspNetCore.Mvc;
using AIFeedback.Services.IServices;
using System.Threading.Tasks;

namespace AIFeedback.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly IGmailService _gmailService;

        public AIController(IAIService aiService, IGmailService gmailService)
        {
            _aiService = aiService;
            _gmailService = gmailService;
        }

        [HttpGet("analyze-latest-unread-email")]
        public async Task<IActionResult> AnalyzeLatestUnreadEmail()
        {
            var emails = await _gmailService.ReadRecentUnreadEmailsAsync(1);
            if (emails.Count == 0)
                return Ok(new { Message = "No unread emails found." });

            var (latestEmailBody, _) = emails[0];
            var analysis = await _aiService.AnalyzeFeedbackAsync(latestEmailBody);

            return Ok(new { Email = latestEmailBody, Analysis = analysis });
        }

        [HttpGet("reply-to-latest-unread-email")]
        public async Task<IActionResult> ReplyToLatestUnreadEmail()
        {
            var emails = await _gmailService.ReadRecentUnreadEmailsAsync(1);
            if (emails.Count == 0)
                return Ok(new { Message = "No unread emails found." });

            var (latestEmailBody, latestEmailId) = emails[0];
            var analysis = await _aiService.AnalyzeFeedbackAsync(latestEmailBody);

            await _gmailService.ReplyToEmailAsync(latestEmailId, analysis);

            return Ok(new { Email = latestEmailBody, Reply = analysis });
        }
    }
}
