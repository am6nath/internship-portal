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

        public bool IsTestPassed { get; set; }

        public decimal? TestScore { get; set; }

        public DateTime? TestPassedAt { get; set; }
    }
}