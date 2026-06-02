namespace InternshipPortal.API.DTOs.Application
{
    public class StudentAssessmentDto
    {
        public Guid ApplicationId { get; set; }

        public Guid StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string StudentEmail { get; set; } = string.Empty;

        public Guid InternshipId { get; set; }

        public string InternshipTitle { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string DisplayStatus { get; set; } = string.Empty;

        public string DisplayPhase { get; set; } = string.Empty;

        public string PreTestStatus { get; set; } = string.Empty;

        public decimal? PreTestScore { get; set; }

        public DateTime? PreTestPassedAt { get; set; }

        public int PreTestAttemptsUsed { get; set; }

        public string PostTestStatus { get; set; } = string.Empty;

        public decimal? PostTestScore { get; set; }

        public DateTime? PostTestPassedAt { get; set; }

        public int PostTestAttemptsUsed { get; set; }

        public decimal? OverallAssessmentScore { get; set; }

        public bool CanTakePreTest { get; set; }

        public bool CanTakePostTest { get; set; }
    }
}
