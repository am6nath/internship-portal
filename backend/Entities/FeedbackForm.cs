
namespace InternshipPortal.API.Entities
{
    public class FeedbackForm : BaseEntity
    {
        // INTERNSHIP
        public Guid InternshipId { get; set; }

        public Internship Internship { get; set; }

        // FORM DETAILS
        public string Title { get; set; }

        public string Description { get; set; }

        // RELEASE MANAGEMENT
        public DateTime ReleaseDate { get; set; }

        public DateTime SubmissionDeadline { get; set; }

        // STATUS
        public bool IsReleased { get; set; }

        public bool IsClosed { get; set; }

        // SETTINGS
        public bool AllowAnonymous { get; set; }

        public int MaxRating { get; set; } = 5;

        public string AllowedStatuses { get; set; } = "Completed";

        // NAVIGATION
        public ICollection<FeedbackSubmission>
            FeedbackSubmissions { get; set; }
                = new List<FeedbackSubmission>();
    }
}

