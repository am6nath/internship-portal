namespace InternshipPortal.API.Entities
{
    public class StudentProfile : BaseEntity
    {
        public Guid UserId { get; set; }

        public string CollegeName { get; set; }

        public string Department { get; set; }

        public decimal CGPA { get; set; }

        public int Backlogs { get; set; }

        public int GraduationYear { get; set; }

        public string Skills { get; set; }

        public string? ResumeUrl { get; set; }

        public string? ProfileImageUrl { get; set; }

        public bool IsProfileComplete { get; set; } = false;

        // Navigation Property
        public ApplicationUser User { get; set; }
    }
}