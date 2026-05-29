namespace InternshipPortal.API.Entities
{
    public class InternshipTestSession : BaseEntity
    {
        public Guid ApplicationId { get; set; }
        public Application Application { get; set; } = null!;

        public Guid StudentId { get; set; }

        public string SessionToken { get; set; } = string.Empty;

        /// <summary>JSON array of questions with correct answers (server-only grading).</summary>
        public string QuestionsJson { get; set; } = string.Empty;

        public int TotalQuestions { get; set; }

        public bool IsSubmitted { get; set; }

        public int? CorrectAnswers { get; set; }

        public decimal? ScorePercent { get; set; }

        public bool IsPassed { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}
