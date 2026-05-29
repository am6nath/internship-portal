using InternshipPortal.API.DTOs.Student;


namespace InternshipPortal.API.Services.Interfaces
{
    public interface IStudentProfileService
    {
        Task<StudentProfileResponseDto> CreateProfileAsync(
            Guid userId,
            CreateStudentProfileDto model);

        Task<StudentProfileResponseDto?> GetProfileAsync(
            Guid userId);

        Task<StudentProfileResponseDto?> UpdateProfileAsync(
            Guid userId,
            UpdateStudentProfileDto model);

        Task<string> UploadResumeAsync(
    Guid userId,
    IFormFile file);

        Task<string> UploadProfileImageAsync(
            Guid userId,
            IFormFile file);

        Task<List<StudentProfileResponseDto>> GetAllProfilesAsync();
    }
}