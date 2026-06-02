namespace InternshipPortal.API.DTOs.Application
{
    public class TestStatusResponseDto
    {
        public bool IsPreTestPassed { get; set; }

        public decimal? PreTestScore { get; set; }

        public DateTime? PreTestPassedAt { get; set; }

        public bool IsTestPassed { get; set; }

        public decimal? TestScore { get; set; }

        public DateTime? TestPassedAt { get; set; }

        public int PassingScorePercent { get; set; }

        public int PreTestMaxAttempts { get; set; }

        public int PreTestAttemptsUsed { get; set; }

        public bool CanTakePreTest { get; set; }

        public int CompletionTestMaxAttempts { get; set; }

        public int CompletionTestAttemptsUsed { get; set; }

        public bool CanTakeCompletionTest { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
