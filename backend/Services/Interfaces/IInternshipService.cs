using InternshipPortal.API.DTOs.Internship;

namespace InternshipPortal.API.Services.Interfaces
{
    public interface IInternshipService
    {
        Task<InternshipResponseDto> CreateInternshipAsync(
            Guid adminId,
            CreateInternshipDto model);

        Task<InternshipResponseDto?> UpdateInternshipAsync(
            Guid internshipId,
            Guid adminId,
            UpdateInternshipDto model);

        Task<bool> DeleteInternshipAsync(
            Guid internshipId,
            Guid adminId);

        Task<InternshipResponseDto?> GetInternshipByIdAsync(
            Guid internshipId);

Task<IEnumerable<InternshipResponseDto>>
    GetAllInternshipsAsync(
        InternshipFilterDto filter);

    }
}