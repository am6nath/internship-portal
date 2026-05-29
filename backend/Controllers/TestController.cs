using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternshipPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        // PUBLIC API
        [HttpGet("public")]
        public IActionResult Public()
        {
            return Ok("Public API Working");
        }

        // AUTHENTICATED USERS
        [Authorize]
        [HttpGet("protected")]
        public IActionResult Protected()
        {
            return Ok("Protected API Working");
        }

        // ADMIN ONLY
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult AdminOnly()
        {
            return Ok("Admin API Working");
        }

        // STUDENT ONLY
        [Authorize(Roles = "Student")]
        [HttpGet("student")]
        public IActionResult StudentOnly()
        {
            return Ok("Student API Working");
        }
    }
}