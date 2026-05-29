using System.Security.Claims;
using InternshipPortal.API.DTOs.Student;
using InternshipPortal.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternshipPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StudentProfileController : ControllerBase
    {
        private readonly IStudentProfileService _studentProfileService;

        public StudentProfileController(
            IStudentProfileService studentProfileService)
        {
            _studentProfileService = studentProfileService;
        }

        // CREATE PROFILE
        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> CreateProfile(
            CreateStudentProfileDto model)
        {
            var userId = GetCurrentUserId();

            var result = await _studentProfileService
                .CreateProfileAsync(userId, model);

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "Profile created successfully",
                data = result
            });
        }

        // GET MY PROFILE
        [Authorize(Roles = "Student")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();

            var result = await _studentProfileService
                .GetProfileAsync(userId);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    statusCode = 404,
                    message = "Profile not found"
                });
            }

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "Profile fetched successfully",
                data = result
            });
        }

        // UPDATE PROFILE
        [Authorize(Roles = "Student")]
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(
            UpdateStudentProfileDto model)
        {
            var userId = GetCurrentUserId();

            var result = await _studentProfileService
                .UpdateProfileAsync(userId, model);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    statusCode = 404,
                    message = "Profile not found"
                });
            }

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "Profile updated successfully",
                data = result
            });
        }

        // UPLOAD RESUME
        [Authorize(Roles = "Student")]
        [HttpPost("upload-resume")]
        public async Task<IActionResult> UploadResume(
            IFormFile file)
        {
            var userId = GetCurrentUserId();

            var result = await _studentProfileService
                .UploadResumeAsync(userId, file);

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "Resume uploaded successfully",

                data = new
                {
                    resumeUrl = result,
                    uploadedAt = DateTime.UtcNow
                }
            });
        }

        // UPLOAD PROFILE IMAGE
        [Authorize(Roles = "Student")]
        [HttpPost("upload-profile-image")]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            var userId = GetCurrentUserId();

            var result = await _studentProfileService
                .UploadProfileImageAsync(userId, file);

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "Profile image uploaded successfully",
                data = new
                {
                    profileImageUrl = result,
                    uploadedAt = DateTime.UtcNow
                }
            });
        }

        // GET ALL STUDENT PROFILES FOR ADMIN
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllProfiles()
        {
            var result = await _studentProfileService.GetAllProfilesAsync();

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "All student profiles fetched successfully",
                data = result
            });
        }

        // GET CURRENT USER ID FROM JWT
        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            return Guid.Parse(userId!);
        }
    }
}