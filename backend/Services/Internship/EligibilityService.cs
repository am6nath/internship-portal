using InternshipPortal.API.Data.Context;
using InternshipPortal.API.DTOs.Internship;
using InternshipPortal.API.Helpers;
using InternshipPortal.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.API.Services.Internship
{
    public class EligibilityService : IEligibilityService
    {
        private readonly ApplicationDbContext _context;

        public EligibilityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<IEnumerable<InternshipResponseDto>>
            GetEligibleInternshipsAsync(Guid studentId) =>
            GetBrowseInternshipsAsync(studentId);

        public async Task<IEnumerable<InternshipResponseDto>>
            GetBrowseInternshipsAsync(Guid studentId)
        {
            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(x =>
                    x.UserId == studentId &&
                    !x.IsDeleted);

            var now = DateTime.UtcNow;

            var internships = await _context.Internships
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var appliedInternshipIds = await _context.Applications
                .Where(x => x.StudentId == studentId && !x.IsDeleted)
                .Select(x => x.InternshipId)
                .ToListAsync();

            var appliedSet = appliedInternshipIds.ToHashSet();

            if (profile == null)
            {
                return MapBrowseList(
                    internships.Where(x => IsVisibleInternship(x, now)),
                    null,
                    appliedSet,
                    "Create your student profile before applying");
            }

            if (!profile.IsProfileComplete)
            {
                return MapBrowseList(
                    internships.Where(x => IsVisibleInternship(x, now)),
                    profile,
                    appliedSet,
                    "Complete your student profile before applying");
            }

            return MapBrowseList(
                internships.Where(x => IsVisibleInternship(x, now)),
                profile,
                appliedSet,
                null);
        }

        public async Task<IEnumerable<InternshipResponseDto>> GetOpenInternshipsAsync()
        {
            var now = DateTime.UtcNow;

            var internships = await _context.Internships
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return MapBrowseList(
                internships.Where(x => IsVisibleInternship(x, now)),
                null,
                new HashSet<Guid>(),
                "Login and complete your profile to apply");
        }

        private static bool IsVisibleInternship(Entities.Internship internship, DateTime now)
        {
            if (internship.IsDeleted || internship.IsExpired)
            {
                return false;
            }

            // Show active internships with open registration
            if (EligibilityHelper.IsOpenForRegistration(internship, now))
            {
                return true;
            }

            // Also show newly created internships that are still marked active
            return internship.IsActive &&
                   internship.AvailableSeats > 0 &&
                   internship.RegistrationDeadline.Date >= now.Date;
        }

        private static IEnumerable<InternshipResponseDto> MapBrowseList(
            IEnumerable<Entities.Internship> internships,
            Entities.StudentProfile? profile,
            HashSet<Guid> appliedSet,
            string? profileBlockReason)
        {
            return internships.Select(internship =>
            {
                string? reason = profileBlockReason;
                var isEligible = false;

                if (profile != null && profile.IsProfileComplete)
                {
                    (isEligible, reason) = EligibilityHelper.CheckStudentEligibility(profile, internship);
                }

                if (!EligibilityHelper.IsOpenForRegistration(internship))
                {
                    isEligible = false;
                    reason ??= "Registration is closed for this internship";
                }

                var hasApplied = appliedSet.Contains(internship.Id);

                return new InternshipResponseDto
                {
                    Id = internship.Id,
                    Title = internship.Title,
                    Description = internship.Description,
                    CompanyName = internship.CompanyName,
                    Location = internship.Location,
                    Stipend = internship.Stipend,
                    DurationInMonths = internship.DurationInMonths,
                    MinimumCGPA = internship.MinimumCGPA,
                    AllowedBacklogs = internship.AllowedBacklogs,
                    RequiredSkills = internship.RequiredSkills,
                    AllowedDepartments = internship.AllowedDepartments,
                    GraduationYear = internship.GraduationYear,
                    TotalSeats = internship.TotalSeats,
                    AvailableSeats = internship.AvailableSeats,
                    IsActive = internship.IsActive,
                    IsExpired = internship.IsExpired,
                    IsSeatsFilled = internship.IsSeatsFilled,
                    StartDate = internship.StartDate,
                    EndDate = internship.EndDate,
                    RegistrationDeadline = internship.RegistrationDeadline,
                    CreatedAt = internship.CreatedAt,
                    UpdatedAt = internship.UpdatedAt,
                    CoverImageUrl = InternshipCoverImageHelper.ResolveCoverImageUrl(internship),
                    IsEligible = isEligible,
                    CanApply = isEligible && !hasApplied,
                    HasApplied = hasApplied,
                    IneligibilityReason = isEligible ? null : reason,
                    IsOpenForRegistration = EligibilityHelper.IsOpenForRegistration(internship)
                };
            });
        }
    }
}
