namespace InternshipPortal.API.DTOs.Feedback
{
    public class CreateFeedbackFormDto
    {
        public Guid InternshipId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime ReleaseDate { get; set; }

        public DateTime SubmissionDeadline { get; set; }

        public bool AllowAnonymous { get; set; }

        public int MaxRating { get; set; } = 5;

        public System.Collections.Generic.List<string> AllowedStatuses { get; set; } = new();
    }
}

