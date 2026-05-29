using InternshipPortal.API.Enums;
namespace InternshipPortal.API.Entities
{
    public class Application : BaseEntity
    {
        // STUDENT
        public Guid StudentId { get; set; }

        public ApplicationUser Student { get; set; }

        // INTERNSHIP
        public Guid InternshipId { get; set; }

        public Internship Internship { get; set; }

        // STATUS
        public ApplicationStatus Status { get; set; }
        // ADMIN RESPONSE
        public string? AdminRemarks { get; set; }

        // TRACKING
        public DateTime AppliedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }
        public bool IsCompleted { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string? CertificateUrl { get; set; }

        // Completion test (Open Trivia DB)
        public bool IsTestPassed { get; set; }

        public decimal? TestScore { get; set; }

        public DateTime? TestPassedAt { get; set; }
    }
}