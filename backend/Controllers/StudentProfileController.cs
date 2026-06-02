using System.Security.Claims;
using InternshipPortal.API.DTOs.Student;
using InternshipPortal.API.Helpers;
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
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> UploadResume(IFormFile? file)
        {
            try
            {
                file ??= FileUploadHelper.ResolveFormFile(
                    Request,
                    "file",
                    "resume",
                    "document",
                    "upload");

                if (file == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        statusCode = 400,
                        message = "No file received. Use multipart/form-data with field name 'file' or 'resume'."
                    });
                }

                var userId = GetCurrentUserId();
                var result = await _studentProfileService.UploadResumeAsync(userId, file);

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
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    statusCode = 400,
                    message = ex.Message
                });
            }
        }

        // UPLOAD PROFILE IMAGE
        [Authorize(Roles = "Student")]
        [HttpPost("upload-profile-image")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadProfileImage(IFormFile? file)
        {
            try
            {
                file ??= FileUploadHelper.ResolveFormFile(
                    Request,
                    "file",
                    "image",
                    "profileImage",
                    "photo",
                    "upload");

                if (file == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        statusCode = 400,
                        message = "No file received. Use multipart/form-data with field name 'file' or 'image'."
                    });
                }

                var userId = GetCurrentUserId();
                var result = await _studentProfileService.UploadProfileImageAsync(userId, file);

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
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    statusCode = 400,
                    message = ex.Message
                });
            }
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

        private Guid GetCurrentUserId()
        {
            return User.GetCurrentUserId();
        }
    }
}
