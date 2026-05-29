using InternshipPortal.API.Data.Context;
using InternshipPortal.API.DTOs.Student;
using InternshipPortal.API.Entities;
using InternshipPortal.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.API.Services.Student
{
    public class StudentProfileService : IStudentProfileService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public StudentProfileService(
            ApplicationDbContext context,
            IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // CREATE PROFILE
        public async Task<StudentProfileResponseDto> CreateProfileAsync(
            Guid userId,
            CreateStudentProfileDto model)
        {
            // CHECK EXISTING PROFILE
            var existingProfile = await _context.StudentProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (existingProfile != null)
            {
                throw new Exception("Profile already exists");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var profile = new StudentProfile
            {
                UserId = userId,
                CollegeName = model.CollegeName,
                Department = model.Department,
                CGPA = model.CGPA,
                Backlogs = model.Backlogs,
                GraduationYear = model.GraduationYear,
                Skills = model.Skills,

                IsProfileComplete = true,

                CreatedBy = userId
            };

            _context.StudentProfiles.Add(profile);

            await _context.SaveChangesAsync();

            return new StudentProfileResponseDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                FullName = user.FullName,
                Email = user.Email,
                CollegeName = profile.CollegeName,
                Department = profile.Department,
                CGPA = profile.CGPA,
                Backlogs = profile.Backlogs,
                GraduationYear = profile.GraduationYear,
                Skills = profile.Skills,
                ResumeUrl = profile.ResumeUrl,
                ProfileImageUrl = profile.ProfileImageUrl,
                IsProfileComplete = profile.IsProfileComplete,

                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };
        }

        // GET PROFILE
        public async Task<StudentProfileResponseDto?> GetProfileAsync(
            Guid userId)
        {
            var profile = await _context.StudentProfiles
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    !x.IsDeleted);

            if (profile == null)
            {
                return null;
            }

            return new StudentProfileResponseDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                FullName = profile.User?.FullName,
                Email = profile.User?.Email,
                CollegeName = profile.CollegeName,
                Department = profile.Department,
                CGPA = profile.CGPA,
                Backlogs = profile.Backlogs,
                GraduationYear = profile.GraduationYear,
                Skills = profile.Skills,
                ResumeUrl = profile.ResumeUrl,
                ProfileImageUrl = profile.ProfileImageUrl,
                IsProfileComplete = profile.IsProfileComplete,

                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };
        }

        // UPDATE PROFILE
        public async Task<StudentProfileResponseDto?> UpdateProfileAsync(
            Guid userId,
            UpdateStudentProfileDto model)
        {
            var profile = await _context.StudentProfiles
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    !x.IsDeleted);

            if (profile == null)
            {
                return null;
            }

            profile.CollegeName = model.CollegeName;
            profile.Department = model.Department;
            profile.CGPA = model.CGPA;
            profile.Backlogs = model.Backlogs;
            profile.GraduationYear = model.GraduationYear;
            profile.Skills = model.Skills;

            profile.UpdatedAt = DateTime.UtcNow;
            profile.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return new StudentProfileResponseDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                FullName = profile.User?.FullName,
                Email = profile.User?.Email,
                CollegeName = profile.CollegeName,
                Department = profile.Department,
                CGPA = profile.CGPA,
                Backlogs = profile.Backlogs,
                GraduationYear = profile.GraduationYear,
                Skills = profile.Skills,
                ResumeUrl = profile.ResumeUrl,
                ProfileImageUrl = profile.ProfileImageUrl,
                IsProfileComplete = profile.IsProfileComplete,

                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };
        }

        // UPLOAD RESUME
        public async Task<string> UploadResumeAsync(
            Guid userId,
            IFormFile file)
        {
            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    !x.IsDeleted);

            if (profile == null)
            {
                throw new Exception("Profile not found");
            }

            // DELETE OLD RESUME
            if (!string.IsNullOrEmpty(profile.ResumeUrl))
            {
                _fileService.DeleteFile(profile.ResumeUrl);
            }

            // UPLOAD NEW RESUME
            var resumePath =
                await _fileService.UploadResumeAsync(file);

            // UPDATE PROFILE
            profile.ResumeUrl = resumePath;

            profile.UpdatedAt = DateTime.UtcNow;
            profile.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return resumePath;
        }

        // UPLOAD PROFILE IMAGE
        public async Task<string> UploadProfileImageAsync(
            Guid userId,
            IFormFile file)
        {
            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    !x.IsDeleted);

            if (profile == null)
            {
                throw new Exception("Profile not found");
            }

            if (!string.IsNullOrEmpty(profile.ProfileImageUrl))
            {
                _fileService.DeleteFile(profile.ProfileImageUrl);
            }

            var imagePath = await _fileService.UploadProfileImageAsync(file);

            profile.ProfileImageUrl = imagePath;
            profile.UpdatedAt = DateTime.UtcNow;
            profile.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return imagePath;
        }

        // GET ALL PROFILES
        public async Task<List<StudentProfileResponseDto>> GetAllProfilesAsync()
        {
            var profiles = await _context.StudentProfiles
                .Include(x => x.User)
                .Where(x => !x.IsDeleted)
                .ToListAsync();

            return profiles.Select(profile => new StudentProfileResponseDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                FullName = profile.User?.FullName,
                Email = profile.User?.Email,
                CollegeName = profile.CollegeName,
                Department = profile.Department,
                CGPA = profile.CGPA,
                Backlogs = profile.Backlogs,
                GraduationYear = profile.GraduationYear,
                Skills = profile.Skills,
                ResumeUrl = profile.ResumeUrl,
                ProfileImageUrl = profile.ProfileImageUrl,
                IsProfileComplete = profile.IsProfileComplete,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            }).ToList();
        }
    }
}