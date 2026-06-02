using InternshipPortal.API.DTOs.Application;
using InternshipPortal.API.Enums;

namespace InternshipPortal.API.Services.Interfaces
{
    public interface IInternshipTestService
    {
        Task<StartTestResponseDto> StartPreTestAsync(Guid applicationId, Guid studentId);
        Task<SubmitTestResultDto> SubmitPreTestAsync(Guid applicationId, Guid studentId, SubmitTestDto model);
        Task<StartTestResponseDto> StartCompletionTestAsync(Guid applicationId, Guid studentId);
        Task<SubmitTestResultDto> SubmitCompletionTestAsync(Guid applicationId, Guid studentId, SubmitTestDto model);
        Task<TestStatusResponseDto?> GetTestStatusAsync(Guid applicationId, Guid studentId);
    }
}
