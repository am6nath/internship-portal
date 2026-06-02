using InternshipPortal.API.Data.Context;
using InternshipPortal.API.DTOs.TrainingMaterial;
using InternshipPortal.API.Entities;
using InternshipPortal.API.Enums;
using InternshipPortal.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.API.Services.TrainingMaterial
{
    public class TrainingMaterialService
        : ITrainingMaterialService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly IFileUrlService _fileUrlService;

        public TrainingMaterialService(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            IEmailService emailService,
            INotificationService notificationService,
            IFileUrlService fileUrlService)
        {
            _context = context;
            _environment = environment;
            _emailService = emailService;
            _notificationService = notificationService;
            _fileUrlService = fileUrlService;
        }



        // UPLOAD MATERIAL
        public async Task UploadMaterialAsync(
            Guid adminId,
            UploadTrainingMaterialDto model)
        {
            // CHECK INTERNSHIP
            var internship =
                await _context.Internships
                    .FirstOrDefaultAsync(x =>
                        x.Id == model.InternshipId &&
                        !x.IsDeleted);

            if (internship == null)
            {
                throw new Exception(
                    "Internship not found"
                );
            }

            // CHECK FILE
            if (model.File == null ||
                model.File.Length == 0)
            {
                throw new Exception(
                    "File is required"
                );
            }
            // ALLOWED FILE TYPES
            var allowedExtensions =
                new[]
                {
                    ".pdf",
                    ".docx",
                    ".jpg",
                    ".jpeg",
                    ".png"
                };

            var extension =
                Path.GetExtension(
                    model.File.FileName)
                .ToLower();

            // INVALID EXTENSION
            if (!allowedExtensions
                    .Contains(extension))
            {
                throw new Exception(
                    "Invalid file type"
                );
            }

            // MAX FILE SIZE (10MB)
            const long maxFileSize =
                10 * 1024 * 1024;

            if (model.File.Length >
                maxFileSize)
            {
                throw new Exception(
                    "File size cannot exceed 10MB"
                );
            }
            if (model.File == null ||
                model.File.Length == 0)
            {
                throw new Exception(
                    "File is required"
                );
            }


            // CREATE FOLDER
            var folderPath =
                Path.Combine(
                    GetWebRootPath(),
                    "training-materials"
                );

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // GENERATE FILE NAME
            var fileName =
                $"{Guid.NewGuid()}" +
                Path.GetExtension(model.File.FileName);

            var filePath =
                Path.Combine(folderPath, fileName);

            // SAVE FILE
            using (var stream =
                   new FileStream(
                       filePath,
                       FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }

            // FILE URL
            var fileUrl =
                $"/training-materials/{fileName}";

            // CREATE ENTITY
            var material =
                new Entities.TrainingMaterial
                {
                    InternshipId =
                        model.InternshipId,

                    Title =
                        model.Title,

                    Description =
                        model.Description,

                    FileUrl =
                        fileUrl,

                    FileType =
                        model.File.ContentType,

                    FileSize =
                        model.File.Length,

                    AcceptedStudentsOnly =
                        model.AcceptedStudentsOnly,

                    CompletedStudentsOnly =
                        model.CompletedStudentsOnly,

                    UploadedBy =
                        adminId,

                    UploadedAt =
                        DateTime.UtcNow,

                    CreatedBy =
                        adminId
                };

            _context.TrainingMaterials
                .Add(material);

            await _context.SaveChangesAsync();
            // GET ELIGIBLE STUDENTS
            var applications =
                await _context.Applications
                    .Include(x => x.Student)
                    .Where(x =>
                        x.InternshipId ==
                            model.InternshipId &&
                        !x.IsDeleted)
                    .ToListAsync();

            foreach (var application in applications)
            {
                // FILTER ACCESS
                if (model.CompletedStudentsOnly)
                {
                    if (application.Status !=
                        ApplicationStatus.Completed)
                    {
                        continue;
                    }
                }
                else if (model.AcceptedStudentsOnly)
                {
                    if (application.Status !=
                            ApplicationStatus.Accepted &&
                        application.Status !=
                            ApplicationStatus.InProgress &&
                        application.Status !=
                            ApplicationStatus.Completed)
                    {
                        continue;
                    }
                }

                // EMAIL SUBJECT
                var subject =
                    $"New Training Material - {internship.Title}";

                // EMAIL BODY
                var body = $@"
Hello {application.Student.FullName},

New training material has been uploaded.

Internship:
{internship.Title}

Material:
{model.Title}

Description:
{model.Description}

Please login to the portal to access the material.

Thank you,
Internship Portal
";

                // SEND EMAIL
                try
                {
                    await _emailService.SendEmailAsync(
                        application.Student.Email!,
                        subject,
                        body
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"EMAIL ERROR: {ex.Message}"
                    );
                }

                // IN-APP NOTIFICATION
                try
                {
                    await _notificationService.CreateNotificationAsync(
                        application.StudentId,
                        "New Training Material",
                        $"New material '{model.Title}' uploaded for '{internship.Title}'.",
                        "Material"
                    );
                }
                catch { }


            }


        }

        // GET INTERNSHIP MATERIALS
        public async Task<IEnumerable<
            TrainingMaterialResponseDto>>
            GetInternshipMaterialsAsync(
                Guid internshipId)
        {
            var materials =
                await _context.TrainingMaterials
                    .Include(x => x.Internship)
                    .Where(x =>
                        x.InternshipId ==
                            internshipId &&
                        !x.IsDeleted)
                    .OrderByDescending(x =>
                        x.UploadedAt)
                    .ToListAsync();

            return materials.Select(x =>
                new TrainingMaterialResponseDto
                {
                    Id = x.Id,

                    InternshipId =
                        x.InternshipId,

                    InternshipTitle =
                        x.Internship.Title,

                    Title =
                        x.Title,

                    Description =
                        x.Description,

                    FileUrl =
                        _fileUrlService.ToPublicUrl(x.FileUrl),

                    FileType =
                        x.FileType,

                    FileSize =
                        x.FileSize,

                    AcceptedStudentsOnly =
                        x.AcceptedStudentsOnly,

                    CompletedStudentsOnly =
                        x.CompletedStudentsOnly,

                    UploadedAt =
                        x.UploadedAt
                });
        }

        // GET STUDENT MATERIALS
        public async Task<IEnumerable<
            TrainingMaterialResponseDto>>
            GetStudentMaterialsAsync(
                Guid studentId)
        {
            // GET STUDENT APPLICATIONS
            var applications =
                await _context.Applications
                    .Include(x => x.Internship)
                    .Where(x =>
                        x.StudentId ==
                            studentId &&
                        !x.IsDeleted)
                    .ToListAsync();

            var internshipIds =
                applications
                    .Select(x => x.InternshipId)
                    .ToList();

            var materials =
                await _context.TrainingMaterials
                    .Include(x => x.Internship)
                    .Where(x =>
                        internshipIds
                            .Contains(
                                x.InternshipId) &&
                        !x.IsDeleted)
                    .ToListAsync();

            var accessibleMaterials =
                new List<Entities.TrainingMaterial>();

            foreach (var material in materials)
            {
                var application =
                    applications.FirstOrDefault(x =>
                        x.InternshipId ==
                            material.InternshipId);

                if (application == null)
                {
                    continue;
                }

                // COMPLETED ONLY
                if (material.CompletedStudentsOnly)
                {
                    if (application.Status ==
                        ApplicationStatus.Completed)
                    {
                        accessibleMaterials
                            .Add(material);
                    }

                    continue;
                }

                // ACCEPTED ONLY
                if (material.AcceptedStudentsOnly)
                {
                    if (application.Status ==
                            ApplicationStatus.Accepted ||
                        application.Status ==
                            ApplicationStatus.InProgress ||
                        application.Status ==
                            ApplicationStatus.Completed)
                    {
                        accessibleMaterials
                            .Add(material);
                    }

                    continue;
                }

                // PUBLIC MATERIAL
                accessibleMaterials
                    .Add(material);
            }

            return accessibleMaterials
                .Select(x =>
                    new TrainingMaterialResponseDto
                    {
                        Id = x.Id,

                        InternshipId =
                            x.InternshipId,

                        InternshipTitle =
                            x.Internship.Title,

                        Title =
                            x.Title,

                        Description =
                            x.Description,

                        FileUrl =
                            _fileUrlService.ToPublicUrl(x.FileUrl),

                        FileType =
                            x.FileType,

                        FileSize =
                            x.FileSize,

                        AcceptedStudentsOnly =
                            x.AcceptedStudentsOnly,

                        CompletedStudentsOnly =
                            x.CompletedStudentsOnly,

                        UploadedAt =
                            x.UploadedAt
                    });
        }

        // DELETE MATERIAL
        public async Task DeleteMaterialAsync(
            Guid materialId,
            Guid adminId)
        {
            var material =
                await _context.TrainingMaterials
                    .FirstOrDefaultAsync(x =>
                        x.Id == materialId &&
                        !x.IsDeleted);

            if (material == null)
            {
                throw new Exception(
                    "Material not found"
                );
            }

            // SOFT DELETE
            material.IsDeleted = true;

            material.DeletedAt =
                DateTime.UtcNow;

            material.DeletedBy =
                adminId;

            await _context.SaveChangesAsync();
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
    }
}

