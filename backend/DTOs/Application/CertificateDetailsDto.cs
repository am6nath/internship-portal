using System;

namespace InternshipPortal.API.DTOs.Application
{
    public class CertificateDetailsDto
    {
        public Guid ApplicationId { get; set; }
        public string StudentName { get; set; }
        public string InternshipTitle { get; set; }
        public string CompanyName { get; set; }
        public int DurationInMonths { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CompletedAt { get; set; }
        public string VerificationCode { get; set; }
        public string? CoverImageUrl { get; set; }
        public bool IsCompleted { get; set; }
    }
}
