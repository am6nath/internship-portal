
namespace InternshipPortal.API.Entities
{
    public class FeedbackSubmission : BaseEntity
    {
        // FEEDBACK FORM
        public Guid FeedbackFormId { get; set; }

        public FeedbackForm FeedbackForm { get; set; }

        // STUDENT
        public Guid StudentId { get; set; }

        public ApplicationUser Student { get; set; }

        // INTERNSHIP APPLICATION
        public Guid ApplicationId { get; set; }

        public Application Application { get; set; }

        // FEEDBACK CONTENT
        public int Rating { get; set; }

        public string Comments { get; set; }

        // TRACKING
        public DateTime SubmittedAt { get; set; }

        public bool IsAnonymous { get; set; }
    }
}

