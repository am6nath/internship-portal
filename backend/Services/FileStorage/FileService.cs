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

        private const long MaxResumeSize = 5 * 1024 * 1024;
        private const long MaxImageSize = 2 * 1024 * 1024;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadResumeAsync(IFormFile file)
        {
            // FILE NULL CHECK
            if (file == null || file.Length == 0)
            {
                throw new Exception("File is required");
            }

            // FILE SIZE CHECK
            if (file.Length > MaxResumeSize)
            {
                throw new Exception("File size exceeds 5MB");
            }

            // EXTENSION CHECK
            var extension = Path.GetExtension(file.FileName)
                .ToLower();

            if (!_allowedResumeExtensions.Contains(extension))
            {
                throw new Exception(
                    "Only PDF, DOC, DOCX files are allowed"
                );
            }

            // UNIQUE FILE NAME
            var fileName =
                $"{Guid.NewGuid()}{extension}";

            // FOLDER PATH
            var folderPath = Path.Combine(
                _environment.WebRootPath,
                "resumes"
            );

            // CREATE FOLDER IF NOT EXISTS
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // FULL FILE PATH
            var filePath = Path.Combine(
                folderPath,
                fileName
            );

            // SAVE FILE
            using (var stream = new FileStream(
                filePath,
                FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // RETURN RELATIVE PATH
            return $"/resumes/{fileName}";
        }

        public async Task<string> UploadProfileImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new Exception("File is required");
            }

            if (file.Length > MaxImageSize)
            {
                throw new Exception("Image size exceeds 2MB");
            }

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!_allowedImageExtensions.Contains(extension))
            {
                throw new Exception("Only JPG, PNG, and WEBP images are allowed");
            }

            var fileName = $"{Guid.NewGuid()}{extension}";

            var folderPath = Path.Combine(
                _environment.WebRootPath,
                "profile-images"
            );

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/profile-images/{fileName}";
        }

        public void DeleteFile(string filePath)
        {
            var fullPath = Path.Combine(
                _environment.WebRootPath,
                filePath.TrimStart('/')
            );

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}