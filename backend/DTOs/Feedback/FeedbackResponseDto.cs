namespace InternshipPortal.API.DTOs.Feedback
{
    public class FeedbackResponseDto
    {
        public Guid Id { get; set; }

        public Guid StudentId { get; set; }

        public string StudentName { get; set; }

        public Guid InternshipId { get; set; }

        public string InternshipTitle { get; set; }

        public int Rating { get; set; }

        public string Comments { get; set; }

        public bool IsAnonymous { get; set; }

        public int MaxRating { get; set; }

        public DateTime SubmittedAt { get; set; }
    }
}

