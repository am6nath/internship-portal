namespace InternshipPortal.API.DTOs.Internship
{
    public class UpdateInternshipDto
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string CompanyName { get; set; }

        public string Location { get; set; }

        public decimal Stipend { get; set; }

        public int DurationInMonths { get; set; }

        // Eligibility
        public decimal MinimumCGPA { get; set; }

        public int AllowedBacklogs { get; set; }

        public string RequiredSkills { get; set; }

        public string AllowedDepartments { get; set; }

        public int GraduationYear { get; set; }

        // Seats
        public int TotalSeats { get; set; }

        // Dates
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime RegistrationDeadline { get; set; }

        public string? CoverImageUrl { get; set; }
    }
}