using InternshipPortal.API.DTOs.Application;

namespace InternshipPortal.API.Services.Interfaces
{
    public interface IInternshipTestService
    {
        Task<StartTestResponseDto> StartTestAsync(Guid applicationId, Guid studentId);
        Task<SubmitTestResultDto> SubmitTestAsync(Guid applicationId, Guid studentId, SubmitTestDto model);
        Task<object?> GetTestStatusAsync(Guid applicationId, Guid studentId);
    }
}
