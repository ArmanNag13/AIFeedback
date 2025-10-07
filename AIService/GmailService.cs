using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Text;
using System.Text.RegularExpressions;
using AIFeedback.Services.IServices;

namespace AIFeedback.Services
{
    public class MyGmailService : IGmailService
    {
        private readonly string[] Scopes = { GmailService.Scope.GmailModify };
        private readonly string ApplicationName = "AI Feedback Analyzer";

        public async Task<List<(string Body, string MessageId)>> ReadRecentUnreadEmailsAsync(int maxResults = 5)
        {
            var emailContents = new List<(string, string)>();

            Console.WriteLine("🔐 Authorizing Gmail access...");
            UserCredential credential;
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
            }

            var gmailService = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var request = gmailService.Users.Messages.List("me");
            request.LabelIds = "UNREAD";
            request.MaxResults = maxResults;

            var messagesResponse = await request.ExecuteAsync();
            if (messagesResponse.Messages == null || messagesResponse.Messages.Count == 0)
            {
                Console.WriteLine("📭 No unread emails found.");
                return emailContents;
            }

            Console.WriteLine($"📧 Found {messagesResponse.Messages.Count} unread message(s).");

            var msgItem = messagesResponse.Messages.First();
            var email = await gmailService.Users.Messages.Get("me", msgItem.Id).ExecuteAsync();
            var body = ExtractEmailBody(email);

            if (!string.IsNullOrEmpty(body))
            {
                emailContents.Add((body, msgItem.Id));

                var mods = new ModifyMessageRequest { RemoveLabelIds = new List<string> { "UNREAD" } };
                await gmailService.Users.Messages.Modify(mods, "me", msgItem.Id).ExecuteAsync();

                Console.WriteLine("✅ Email body extracted.");
            }

            return emailContents;
        }

        public async Task ReplyToEmailAsync(string messageId, string replyText)
        {
            using var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read);
            string credPath = "token.json";

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true));

            var gmailService = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var originalMessage = await gmailService.Users.Messages.Get("me", messageId).ExecuteAsync();

            var reply = new Message
            {
                Raw = EncodeBase64($"To: {GetHeader(originalMessage, "From")}\r\n" +
                                   $"Subject: Re: {GetHeader(originalMessage, "Subject")}\r\n" +
                                   $"In-Reply-To: {messageId}\r\n" +
                                   $"References: {messageId}\r\n\r\n" +
                                   replyText)
            };

            await gmailService.Users.Messages.Send(reply, "me").ExecuteAsync();
        }

        private string EncodeBase64(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').Replace("=", "");
        }

        private string GetHeader(Message message, string headerName)
        {
            return message.Payload.Headers?.FirstOrDefault(h => h.Name == headerName)?.Value ?? "";
        }

        private string ExtractEmailBody(Message email)
        {
            if (email?.Payload == null)
                return string.Empty;

            string GetBody(MessagePart part)
            {
                if (part == null)
                    return string.Empty;

                if (!string.IsNullOrEmpty(part.Body?.Data))
                {
                    if (part.MimeType == "text/plain" || part.MimeType == "text/html")
                    {
                        var decoded = DecodeBase64(part.Body.Data);
                        if (part.MimeType == "text/html")
                            decoded = StripHtmlTags(decoded);
                        return decoded;
                    }
                }

                if (part.Parts != null && part.Parts.Count > 0)
                {
                    foreach (var subPart in part.Parts)
                    {
                        var result = GetBody(subPart);
                        if (!string.IsNullOrEmpty(result))
                            return result;
                    }
                }

                return string.Empty;
            }

            return GetBody(email.Payload);
        }

        private string DecodeBase64(string base64Data)
        {
            if (string.IsNullOrEmpty(base64Data))
                return string.Empty;

            base64Data = base64Data.Replace("-", "+").Replace("_", "/");
            var bytes = Convert.FromBase64String(base64Data);
            return Encoding.UTF8.GetString(bytes);
        }

        private string StripHtmlTags(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return Regex.Replace(input, "<.*?>", string.Empty)
                         .Replace("&nbsp;", " ")
                         .Replace("&lt;", "<")
                         .Replace("&gt;", ">")
                         .Replace("&amp;", "&")
                         .Trim();
        }

  
    }
}
