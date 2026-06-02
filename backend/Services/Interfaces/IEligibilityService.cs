using InternshipPortal.API.DTOs.Internship;

namespace InternshipPortal.API.Services.Interfaces
{
    public interface IEligibilityService
    {
        Task<IEnumerable<InternshipResponseDto>>
            GetEligibleInternshipsAsync(
                Guid studentId);

        Task<IEnumerable<InternshipResponseDto>>
            GetBrowseInternshipsAsync(
                Guid studentId);

        Task<IEnumerable<InternshipResponseDto>>
            GetOpenInternshipsAsync();
    }
}