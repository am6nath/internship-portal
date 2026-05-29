namespace InternshipPortal.API.DTOs.Feedback
{
    public class SubmitFeedbackDto
    {
        public Guid FeedbackFormId { get; set; }

        public Guid ApplicationId { get; set; }

        public int Rating { get; set; }

        public string Comments { get; set; }

        public bool IsAnonymous { get; set; }
    }
}

