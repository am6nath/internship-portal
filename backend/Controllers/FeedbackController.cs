using System.Security.Claims;
using InternshipPortal.API.DTOs.Feedback;
using InternshipPortal.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternshipPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        // CREATE FEEDBACK FORM
        [Authorize(Roles = "Admin")]
        [HttpPost("create-form")]
        public async Task<IActionResult> CreateFeedbackForm(CreateFeedbackFormDto model)
        {
            var adminId = GetCurrentUserId();
            await _feedbackService.CreateFeedbackFormAsync(adminId, model);

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "Feedback form created successfully"
            });
        }

        // SUBMIT FEEDBACK
        [Authorize(Roles = "Student")]
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitFeedback(SubmitFeedbackDto model)
        {
            var studentId = GetCurrentUserId();
            await _feedbackService.SubmitFeedbackAsync(studentId, model);

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "Feedback submitted successfully"
            });
        }

        // GET INTERNSHIP FEEDBACKS
        [Authorize(Roles = "Admin")]
        [HttpGet("internship/{internshipId:guid}")]
        public async Task<IActionResult> GetInternshipFeedbacks(Guid internshipId)
        {
            var result = await _feedbackService.GetInternshipFeedbacksAsync(internshipId);

            return Ok(new
            {
                success = true,
                statusCode = 200,
                data = result
            });
        }

        // GET STUDENT FEEDBACKS
        [Authorize(Roles = "Student")]
        [HttpGet("my-feedbacks")]
        public async Task<IActionResult> GetStudentFeedbacks()
        {
            var studentId = GetCurrentUserId();
            var result = await _feedbackService.GetStudentFeedbacksAsync(studentId);

            return Ok(new
            {
                success = true,
                statusCode = 200,
                data = result
            });
        }

        // GET FEEDBACK FORMS FOR INTERNSHIP
        [Authorize]
        [HttpGet("forms/internship/{internshipId:guid}")]
        public async Task<IActionResult> GetFeedbackForms(Guid internshipId)
        {
            var result = await _feedbackService.GetFeedbackFormsAsync(internshipId);

            return Ok(new
            {
                success = true,
                statusCode = 200,
                data = result
            });
        }

        // GET CURRENT USER ID
        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userId!);
        }
    }
}
