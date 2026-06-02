using System.Security.Claims;
using InternshipPortal.API.DTOs.Application;
using InternshipPortal.API.Helpers;
using InternshipPortal.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternshipPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService _applicationService;
        private readonly IInternshipTestService _internshipTestService;

        public ApplicationController(
            IApplicationService applicationService,
            IInternshipTestService internshipTestService)
        {
            _applicationService = applicationService;
            _internshipTestService = internshipTestService;
        }

        // APPLY INTERNSHIP
        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> ApplyInternship(ApplyInternshipDto model)
        {
            var studentId = GetCurrentUserId();

            try
            {
                var result = await _applicationService.ApplyInternshipAsync(studentId, model);

                return Ok(new
                {
                    success = true,
                    statusCode = 200,
                    message = "Applied successfully",
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

        // GET STUDENT APPLICATIONS
        [Authorize(Roles = "Student")]
        [HttpGet("my-applications")]
        public async Task<IActionResult> GetStudentApplications()
        {
            try
            {
                var studentId = GetCurrentUserId();
                var result = await _applicationService.GetStudentApplicationsAsync(studentId);

                return Ok(new
                {
                    success = true,
                    statusCode = 200,
                    message = "Applications fetched successfully",
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

        // GET INTERNSHIP APPLICATIONS
        [Authorize(Roles = "Admin")]
        [HttpGet("internship/{internshipId:guid}")]
        public async Task<IActionResult> GetInternshipApplications(Guid internshipId)
        {
            var result = await _applicationService.GetInternshipApplicationsAsync(internshipId);

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "Applications fetched successfully",
                data = result
            });
        }

        // UPDATE APPLICATION STATUS
        [Authorize(Roles = "Admin")]
        [HttpPut("{applicationId:guid}/status")]
        public async Task<IActionResult> UpdateApplicationStatus(Guid applicationId, UpdateApplicationStatusDto model)
        {
            var adminId = GetCurrentUserId();
            var result = await _applicationService.UpdateApplicationStatusAsync(applicationId, adminId, model);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    statusCode = 404,
                    message = "Application not found"
                });
            }

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "Application status updated successfully",
                data = result
            });
        }

        // COMPLETE INTERNSHIP
        [Authorize(Roles = "Admin")]
        [HttpPut("{applicationId:guid}/complete")]
        public async Task<IActionResult> CompleteInternship(Guid applicationId, CompleteInternshipDto model)
        {
            var adminId = GetCurrentUserId();
            var result = await _applicationService.CompleteInternshipAsync(applicationId, adminId, model);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    statusCode = 404,
                    message = "Application not found"
                });
            }

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "Internship marked completed",
                data = result
            });
        }

        // START PRE-TEST (one attempt, required after acceptance)
        [Authorize(Roles = "Student")]
        [HttpPost("{applicationId:guid}/pretest/start")]
        public async Task<IActionResult> StartPreTest(Guid applicationId)
        {
            try
            {
                var studentId = GetCurrentUserId();
                var result = await _internshipTestService.StartPreTestAsync(applicationId, studentId);
                return Ok(new
                {
                    success = true,
                    statusCode = 200,
                    message = "Pre-test started successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, statusCode = 400, message = ex.Message });
            }
        }

        // SUBMIT PRE-TEST
        [Authorize(Roles = "Student")]
        [HttpPost("{applicationId:guid}/pretest/submit")]
        public async Task<IActionResult> SubmitPreTest(
            Guid applicationId,
            SubmitTestDto model)
        {
            try
            {
                var studentId = GetCurrentUserId();
                var result = await _internshipTestService.SubmitPreTestAsync(applicationId, studentId, model);
                return Ok(new
                {
                    success = true,
                    statusCode = 200,
                    message = result.Message,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, statusCode = 400, message = ex.Message });
            }
        }

        // START COMPLETION TEST
        [Authorize(Roles = "Student")]
        [HttpPost("{applicationId:guid}/test/start")]
        public async Task<IActionResult> StartCompletionTest(Guid applicationId)
        {
            try
            {
                var studentId = GetCurrentUserId();
                var result = await _internshipTestService.StartCompletionTestAsync(applicationId, studentId);
                return Ok(new
                {
                    success = true,
                    statusCode = 200,
                    message = "Completion test started successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, statusCode = 400, message = ex.Message });
            }
        }

        // SUBMIT COMPLETION TEST
        [Authorize(Roles = "Student")]
        [HttpPost("{applicationId:guid}/test/submit")]
        public async Task<IActionResult> SubmitCompletionTest(
            Guid applicationId,
            SubmitTestDto model)
        {
            try
            {
                var studentId = GetCurrentUserId();
                var result = await _internshipTestService.SubmitCompletionTestAsync(applicationId, studentId, model);
                return Ok(new
                {
                    success = true,
                    statusCode = 200,
                    message = result.Message,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, statusCode = 400, message = ex.Message });
            }
        }

        // GET TEST STATUS (pre-test + completion test)
        [Authorize(Roles = "Student")]
        [HttpGet("{applicationId:guid}/test/status")]
        public async Task<IActionResult> GetTestStatus(Guid applicationId)
        {
            var studentId = GetCurrentUserId();
            var result = await _internshipTestService.GetTestStatusAsync(applicationId, studentId);

            if (result == null)
            {
                return NotFound(new { success = false, statusCode = 404, message = "Application not found" });
            }

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "Test status fetched",
                data = result
            });
        }

        // GET STUDENT ASSESSMENT (Admin)
        [Authorize(Roles = "Admin")]
        [HttpGet("{applicationId:guid}/assessment")]
        public async Task<IActionResult> GetStudentAssessment(Guid applicationId)
        {
            try
            {
                var adminId = GetCurrentUserId();
                var result = await _applicationService.GetStudentAssessmentAsync(applicationId, adminId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        statusCode = 404,
                        message = "Application not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    statusCode = 200,
                    message = "Assessment fetched successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, statusCode = 400, message = ex.Message });
            }
        }

        // GET PUBLIC CERTIFICATE DETAILS
        [AllowAnonymous]
        [HttpGet("certificate/{applicationId:guid}")]
        public async Task<IActionResult> GetCertificateDetails(Guid applicationId)
        {
            try
            {
                var result = await _applicationService.GetCertificateDetailsAsync(applicationId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        statusCode = 404,
                        message = "Certificate details not found or internship is not completed yet"
                    });
                }

                return Ok(new
                {
                    success = true,
                    statusCode = 200,
                    message = "Certificate details retrieved successfully",
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

        // GET CURRENT USER ID
        private Guid GetCurrentUserId()
        {
            return User.GetCurrentUserId();
        }
    }
}
