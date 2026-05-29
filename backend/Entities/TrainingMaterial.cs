namespace InternshipPortal.API.Entities
{
    public class TrainingMaterial : BaseEntity
    {
        // INTERNSHIP
        public Guid InternshipId { get; set; }

        public Internship Internship { get; set; }

        // MATERIAL DETAILS
        public string Title { get; set; }

        public string Description { get; set; }

        public string FileUrl { get; set; }

        public string FileType { get; set; }

        public long FileSize { get; set; }

        // ACCESS CONTROL
        public bool AcceptedStudentsOnly { get; set; }

        public bool CompletedStudentsOnly { get; set; }

        // TRACKING
        public Guid UploadedBy { get; set; }

        public DateTime UploadedAt { get; set; }
    }
}

