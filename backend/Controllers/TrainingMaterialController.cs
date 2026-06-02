using System.Security.Claims;
using InternshipPortal.API.DTOs.TrainingMaterial;
using InternshipPortal.API.Helpers;
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
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(15 * 1024 * 1024)]
        public async Task<IActionResult>
            UploadMaterial(
                [FromForm]
                UploadTrainingMaterialDto model)
        {
            try
            {
                model.File ??= FileUploadHelper.ResolveFormFile(
                    Request,
                    "file",
                    "document",
                    "upload");

                if (model.File == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        statusCode = 400,
                        message = "No file received. Use multipart/form-data with field name 'file'."
                    });
                }

                var adminId = GetCurrentUserId();

                await _trainingMaterialService
                    .UploadMaterialAsync(
                        adminId,
                        model);

                return Ok(new
                {
                    success = true,
                    statusCode = 200,
                    message = "Training material uploaded successfully"
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
            return User.GetCurrentUserId();
        }
    }
}

