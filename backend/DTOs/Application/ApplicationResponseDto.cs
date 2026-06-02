namespace InternshipPortal.API.DTOs.Application
{
    public class ApplicationResponseDto
    {
        public Guid Id { get; set; }

        // STUDENT
        public Guid StudentId { get; set; }

        public string StudentName { get; set; }

        // INTERNSHIP
        public Guid InternshipId { get; set; }

        public string InternshipTitle { get; set; }

        public string CompanyName { get; set; }

        // STATUS
        public string Status { get; set; }

        public string DisplayStatus { get; set; } = string.Empty;

        public string DisplayPhase { get; set; } = string.Empty;

        public string? AdminRemarks { get; set; }

        // DATES
        public DateTime AppliedAt { get; set; }

        public DateTime? ReviewedAt { get; set; }

        // AUDIT
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string? CertificateUrl { get; set; }

        // Pre-test assessment
        public bool IsPreTestPassed { get; set; }

        public decimal? PreTestScore { get; set; }

        public DateTime? PreTestPassedAt { get; set; }

        public string PreTestStatus { get; set; } = string.Empty;

        public int PreTestAttemptsUsed { get; set; }

        // Post-test (completion) assessment
        public bool IsTestPassed { get; set; }

        public decimal? TestScore { get; set; }

        public decimal? PostTestScore { get; set; }

        public DateTime? TestPassedAt { get; set; }

        public string PostTestStatus { get; set; } = string.Empty;

        public int PostTestAttemptsUsed { get; set; }

        public decimal? OverallAssessmentScore { get; set; }

        public bool CanTakePreTest { get; set; }

        public bool CanTakeCompletionTest { get; set; }

        public bool CanTakePostTest { get; set; }

        public string? CoverImageUrl { get; set; }
    }
}
