using InternshipPortal.API.DTOs.Application;

namespace InternshipPortal.API.Services.Interfaces
{
    public interface IApplicationService
    {
        Task<ApplicationResponseDto>
            ApplyInternshipAsync(
                Guid studentId,
                ApplyInternshipDto model);

        Task<IEnumerable<ApplicationResponseDto>>
            GetStudentApplicationsAsync(
                Guid studentId);

        Task<IEnumerable<ApplicationResponseDto>>
            GetInternshipApplicationsAsync(
                Guid internshipId);

        Task<ApplicationResponseDto?>
            UpdateApplicationStatusAsync(
                Guid applicationId,
                Guid adminId,
                UpdateApplicationStatusDto model);

        Task<ApplicationResponseDto?>
            CompleteInternshipAsync(
                Guid applicationId,
                Guid adminId,
                CompleteInternshipDto model);

        Task<CertificateDetailsDto?>
            GetCertificateDetailsAsync(
                Guid applicationId);
    }
}