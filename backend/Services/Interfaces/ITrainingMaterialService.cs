using InternshipPortal.API.DTOs.TrainingMaterial;

namespace InternshipPortal.API.Services.Interfaces
{
    public interface ITrainingMaterialService
    {
        // ADMIN
        Task UploadMaterialAsync(
            Guid adminId,
            UploadTrainingMaterialDto model);

        // GET MATERIALS FOR INTERNSHIP
        Task<IEnumerable<TrainingMaterialResponseDto>>
            GetInternshipMaterialsAsync(
                Guid internshipId);

        // STUDENT MATERIALS
        Task<IEnumerable<TrainingMaterialResponseDto>>
            GetStudentMaterialsAsync(
                Guid studentId);

        // DELETE MATERIAL
        Task DeleteMaterialAsync(
            Guid materialId,
            Guid adminId);
    }
}

