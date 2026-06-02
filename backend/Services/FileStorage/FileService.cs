using InternshipPortal.API.Services.Interfaces;

namespace InternshipPortal.API.Services.File
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;

        private readonly string[] _allowedResumeExtensions =
        {
            ".pdf",
            ".doc",
            ".docx"
        };

        private readonly string[] _allowedImageExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        private const long MaxResumeSize = 10 * 1024 * 1024;
        private const long MaxImageSize = 5 * 1024 * 1024;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadResumeAsync(IFormFile file)
        {
            ValidateFile(file, _allowedResumeExtensions, MaxResumeSize, "resume");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{extension}";
            var folderPath = Path.Combine(GetWebRootPath(), "resumes");

            return await SaveFileAsync(file, folderPath, fileName, "/resumes");
        }

        public async Task<string> UploadProfileImageAsync(IFormFile file)
        {
            ValidateFile(file, _allowedImageExtensions, MaxImageSize, "image");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{extension}";
            var folderPath = Path.Combine(GetWebRootPath(), "profile-images");

            return await SaveFileAsync(file, folderPath, fileName, "/profile-images");
        }

        public async Task<string> UploadCoverImageAsync(IFormFile file)
        {
            ValidateFile(file, _allowedImageExtensions, MaxImageSize, "image");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{extension}";
            var folderPath = Path.Combine(GetWebRootPath(), "cover-images");

            return await SaveFileAsync(file, folderPath, fileName, "/cover-images");
        }

        public void DeleteFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            var relativePath = filePath;
            if (filePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                filePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    relativePath = new Uri(filePath).AbsolutePath;
                }
                catch
                {
                    return;
                }
            }

            var fullPath = Path.Combine(
                GetWebRootPath(),
                relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
            );

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        private string GetWebRootPath()
        {
            var path = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(path))
            {
                path = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            Directory.CreateDirectory(path);
            return path;
        }

        private static void ValidateFile(
            IFormFile file,
            string[] allowedExtensions,
            long maxSize,
            string fileLabel)
        {
            if (file == null || file.Length == 0)
            {
                throw new Exception("File is required. Send multipart/form-data with field name 'file'.");
            }

            if (file.Length > maxSize)
            {
                throw new Exception($"File size exceeds {maxSize / (1024 * 1024)}MB");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            {
                throw new Exception(
                    fileLabel == "resume"
                        ? "Only PDF, DOC, and DOCX files are allowed"
                        : "Only JPG, PNG, and WEBP images are allowed");
            }
        }

        private static async Task<string> SaveFileAsync(
            IFormFile file,
            string folderPath,
            string fileName,
            string urlPrefix)
        {
            Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"{urlPrefix}/{fileName}";
        }
    }
}
