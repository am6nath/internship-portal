using InternshipPortal.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InternshipPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestEmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public TestEmailController(
            IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> SendTestEmail(
            string toEmail)
        {
            try
            {
                var subject =
                    "Internship Portal Test Email";

                var body = @"
                    <h2>Email Service Working</h2>
                    <p>
                        Your Brevo SMTP integration
                        is working successfully.
                    </p>
                ";

                await _emailService.SendEmailAsync(
                    toEmail,
                    subject,
                    body
                );

                return Ok(new
                {
                    success = true,
                    statusCode = 200,
                    message = "Email sent successfully"
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
    }
}