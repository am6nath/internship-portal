namespace InternshipPortal.API.Services.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadResumeAsync(IFormFile file);

        Task<string> UploadProfileImageAsync(IFormFile file);

        void DeleteFile(string filePath);
    }
}