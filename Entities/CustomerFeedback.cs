using System;

namespace AIFeedback.Entities
{
    public class CustomerFeedback
    {
        public int Id { get; set; } // Primary key
        public string SenderEmail { get; set; } // Email of sender
        public string Message { get; set; } // Email body text
        public string Sentiment { get; set; } // AI-analyzed sentiment
        public string Category { get; set; } // AI-detected category
        public string KeyPhrases { get; set; } // Comma-separated key phrases
        public DateTime ReceivedDate { get; set; } // When email was received
    }
}
