using System.Security.Claims;
using InternshipPortal.API.DTOs.Internship;
using InternshipPortal.API.Helpers;
using InternshipPortal.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternshipPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InternshipController : ControllerBase
    {
        private readonly IInternshipService _internshipService;
        private readonly IEligibilityService _eligibilityService;

        public InternshipController(
            IInternshipService internshipService,
            IEligibilityService eligibilityService)
        {
            _internshipService = internshipService;
            _eligibilityService = eligibilityService;
        }

        // CREATE INTERNSHIP
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateInternship(
            CreateInternshipDto model)
        {
            var adminId = GetCurrentUserId();

            var result = await _internshipService
                .CreateInternshipAsync(adminId, model);

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "Internship created successfully",
                data = result
            });
        }

        // UPDATE INTERNSHIP
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateInternship(
            Guid id,
            UpdateInternshipDto model)
        {
            var adminId = GetCurrentUserId();

            var result = await _internshipService
                .UpdateInternshipAsync(
                    id,
                    adminId,
                    model);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    statusCode = 404,
                    message = "Internship not found"
                });
            }

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "Internship updated successfully",
                data = result
            });
        }

        // DELETE INTERNSHIP
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteInternship(
            Guid id)
        {
            var adminId = GetCurrentUserId();

            var result = await _internshipService
                .DeleteInternshipAsync(
                    id,
                    adminId);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    statusCode = 404,
                    message = "Internship not found"
                });
            }

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "Internship deleted successfully"
            });
        }

        // GET ELIGIBLE / BROWSE INTERNSHIPS (all open internships with eligibility flags)
        [Authorize(Roles = "Student")]
        [HttpGet("eligible")]
        public async Task<IActionResult>
            GetEligibleInternships()
        {
            try
            {
                var studentId = GetCurrentUserId();

                var result = await _eligibilityService
                    .GetBrowseInternshipsAsync(studentId);

                return Ok(new
                {
                    success = true,
                    statusCode = 200,
                    message = "Internships fetched successfully",
                    data = result
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

        [Authorize(Roles = "Student")]
        [HttpGet("browse")]
        public async Task<IActionResult> BrowseInternships()
        {
            var studentId = GetCurrentUserId();
            var result = await _eligibilityService.GetBrowseInternshipsAsync(studentId);

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "Internships fetched successfully",
                data = result
            });
        }

        // GET INTERNSHIP BY ID
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetInternshipById(
            Guid id)
        {
            var result = await _internshipService
                .GetInternshipByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    statusCode = 404,
                    message = "Internship not found"
                });
            }

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "Internship fetched successfully",
                data = result
            });
        }

        // GET ALL INTERNSHIPS
        [HttpGet]
        public async Task<IActionResult>
            GetAllInternships(
                [FromQuery]
        InternshipFilterDto filter)
        {
            var result =
                await _internshipService
                    .GetAllInternshipsAsync(
                        filter);

            return Ok(new
            {
                success = true,
                statusCode = 200,
                data = result
            });
        }

        // PUBLIC OPEN INTERNSHIPS (no login required)
        [AllowAnonymous]
        [HttpGet("open")]
        public async Task<IActionResult> GetOpenInternships()
        {
            var result = await _eligibilityService.GetOpenInternshipsAsync();

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "Open internships fetched successfully",
                data = result
            });
        }

        // UPLOAD COVER IMAGE
        [Authorize(Roles = "Admin")]
        [HttpPost("{id:guid}/upload-cover")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadCoverImage(
            Guid id,
            IFormFile? file)
        {
            try
            {
                file ??= FileUploadHelper.ResolveFormFile(
                    Request,
                    "file",
                    "coverImage",
                    "image",
                    "upload");

                if (file == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        statusCode = 400,
                        message = "No file received. Use multipart/form-data with field name 'file'."
                    });
                }

                var adminId = GetCurrentUserId();
                var result = await _internshipService.UploadCoverImageAsync(id, adminId, file);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        statusCode = 404,
                        message = "Internship not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    statusCode = 200,
                    message = "Cover image uploaded successfully",
                    data = result
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


        // GET CURRENT USER ID FROM JWT
        private Guid GetCurrentUserId()
        {
            return User.GetCurrentUserId();
        }
    }
}
