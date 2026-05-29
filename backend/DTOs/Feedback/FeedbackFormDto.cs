using System;

namespace InternshipPortal.API.DTOs.Feedback
{
    public class FeedbackFormDto
    {
        public Guid Id { get; set; }
        public Guid InternshipId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime ReleaseDate { get; set; }
        public DateTime SubmissionDeadline { get; set; }
        public bool IsReleased { get; set; }
        public bool IsClosed { get; set; }
        public bool AllowAnonymous { get; set; }
        public int MaxRating { get; set; }
        public System.Collections.Generic.List<string> AllowedStatuses { get; set; } = new();
    }
}
