using Microsoft.AspNetCore.Http;

namespace InternshipPortal.API.DTOs.TrainingMaterial
{
    public class UploadTrainingMaterialDto
    {
        public Guid InternshipId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool AcceptedStudentsOnly { get; set; }

        public bool CompletedStudentsOnly { get; set; }

        public IFormFile File { get; set; }
    }
}
