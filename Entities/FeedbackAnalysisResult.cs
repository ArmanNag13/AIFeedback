namespace YourProject.Entities
{
    public class FeedbackAnalysisResult
    {
        public string Sentiment { get; set; }
        public string Category { get; set; }
        public string[] KeyPhrases { get; set; }
    }
}
