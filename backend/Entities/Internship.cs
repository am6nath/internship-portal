namespace InternshipPortal.API.Entities
{
    public class Internship : BaseEntity
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

        public int AvailableSeats { get; set; }

        // Dates
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime RegistrationDeadline { get; set; }

        // Status
        public bool IsActive { get; set; } = true;

        public bool IsExpired { get; set; } = false;

        public bool IsSeatsFilled { get; set; } = false;

        public string? CoverImageUrl { get; set; }

        // Created By Admin
        public Guid AdminId { get; set; }

        public ApplicationUser Admin { get; set; }
        public ICollection<Application>
    Applications
        { get; set; }
        = new List<Application>();

        // FEEDBACK FORMS
        public ICollection<FeedbackForm>
            FeedbackForms
        { get; set; }
                = new List<FeedbackForm>();

        public ICollection<TrainingMaterial>
    TrainingMaterials
        { get; set; }
        = new List<TrainingMaterial>();
    }
}