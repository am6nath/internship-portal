namespace InternshipPortal.API.DTOs.Student
{
    public class StudentProfileResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }

        public string CollegeName { get; set; }

        public string Department { get; set; }

        public decimal CGPA { get; set; }

        public int Backlogs { get; set; }

        public int GraduationYear { get; set; }

        public string Skills { get; set; }

        public string? ResumeUrl { get; set; }

        public string? ProfileImageUrl { get; set; }

        public bool IsProfileComplete { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}