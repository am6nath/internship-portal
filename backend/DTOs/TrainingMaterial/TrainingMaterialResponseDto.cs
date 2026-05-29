namespace InternshipPortal.API.DTOs.TrainingMaterial
{
    public class TrainingMaterialResponseDto
    {
        public Guid Id { get; set; }

        public Guid InternshipId { get; set; }

        public string InternshipTitle { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string FileUrl { get; set; }

        public string FileType { get; set; }

        public long FileSize { get; set; }

        public bool AcceptedStudentsOnly { get; set; }

        public bool CompletedStudentsOnly { get; set; }

        public DateTime UploadedAt { get; set; }
    }
}

