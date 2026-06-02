namespace InternshipPortal.API.Services.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadResumeAsync(IFormFile file);

        Task<string> UploadProfileImageAsync(IFormFile file);

        Task<string> UploadCoverImageAsync(IFormFile file);

        void DeleteFile(string filePath);
    }
}