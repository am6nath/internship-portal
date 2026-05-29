using System.Security.Claims;
using InternshipPortal.API.DTOs.TrainingMaterial;
using InternshipPortal.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternshipPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainingMaterialController
        : ControllerBase
    {
        private readonly
            ITrainingMaterialService
            _trainingMaterialService;

        public TrainingMaterialController(
            ITrainingMaterialService
                trainingMaterialService)
        {
            _trainingMaterialService =
                trainingMaterialService;
        }

        // UPLOAD MATERIAL
        [Authorize(Roles = "Admin")]
        [HttpPost("upload")]
        public async Task<IActionResult>
            UploadMaterial(
                [FromForm]
                UploadTrainingMaterialDto model)
        {
            var adminId =
                GetCurrentUserId();

            await _trainingMaterialService
                .UploadMaterialAsync(
                    adminId,
                    model);

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message =
                    "Training material uploaded successfully"
            });
        }

        // GET INTERNSHIP MATERIALS
        [Authorize]
        [HttpGet("internship/{internshipId:guid}")]
        public async Task<IActionResult>
            GetInternshipMaterials(
                Guid internshipId)
        {
            var result =
                await _trainingMaterialService
                    .GetInternshipMaterialsAsync(
                        internshipId);

            return Ok(new
            {
                success = true,
                statusCode = 200,
                data = result
            });
        }

        // GET STUDENT MATERIALS
        [Authorize(Roles = "Student")]
        [HttpGet("my-materials")]
        public async Task<IActionResult>
            GetStudentMaterials()
        {
            var studentId =
                GetCurrentUserId();

            var result =
                await _trainingMaterialService
                    .GetStudentMaterialsAsync(
                        studentId);

            return Ok(new
            {
                success = true,
                statusCode = 200,
                data = result
            });
        }

        // DELETE MATERIAL
        [Authorize(Roles = "Admin")]
        [HttpDelete("{materialId:guid}")]
        public async Task<IActionResult>
            DeleteMaterial(
                Guid materialId)
        {
            var adminId =
                GetCurrentUserId();

            await _trainingMaterialService
                .DeleteMaterialAsync(
                    materialId,
                    adminId);

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message =
                    "Material deleted successfully"
            });
        }

        // GET CURRENT USER ID
        private Guid GetCurrentUserId()
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            return Guid.Parse(userId!);
        }
    }
}

