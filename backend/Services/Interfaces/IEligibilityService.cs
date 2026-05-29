using InternshipPortal.API.DTOs.Internship;

namespace InternshipPortal.API.Services.Interfaces
{
    public interface IEligibilityService
    {
        Task<IEnumerable<InternshipResponseDto>>
            GetEligibleInternshipsAsync(
                Guid studentId);
    }
}