using InternshipPortal.API.Data.Context;
using InternshipPortal.API.DTOs.Internship;
using InternshipPortal.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.API.Services.Internship
{
    public class EligibilityService : IEligibilityService
    {
        private readonly ApplicationDbContext _context;

        public EligibilityService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InternshipResponseDto>>
            GetEligibleInternshipsAsync(
                Guid studentId)
        {
            // GET STUDENT PROFILE
            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(x =>
                    x.UserId == studentId &&
                    !x.IsDeleted);

            if (profile == null)
            {
                throw new Exception(
                    "Student profile not found"
                );
            }

            // PROFILE COMPLETION CHECK
            if (!profile.IsProfileComplete)
            {
                throw new Exception(
                    "Complete your profile before viewing internships"
                );
            }

            // GET ELIGIBLE INTERNSHIPS
            var now = DateTime.UtcNow;

            var internships = await _context.Internships
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive &&
                    !x.IsExpired &&
                    !x.IsSeatsFilled &&
                    x.RegistrationDeadline >= now &&

                    // CGPA
                    profile.CGPA >= x.MinimumCGPA &&

                    // BACKLOGS
                    profile.Backlogs <= x.AllowedBacklogs &&

                    // DEPARTMENT
                    x.AllowedDepartments.Contains(
                        profile.Department) &&

                    // GRADUATION YEAR
                    profile.GraduationYear ==
                        x.GraduationYear
                )
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return internships.Select(internship =>
                new InternshipResponseDto
                {
                    Id = internship.Id,

                    Title = internship.Title,
                    Description = internship.Description,
                    CompanyName = internship.CompanyName,
                    Location = internship.Location,
                    Stipend = internship.Stipend,
                    DurationInMonths =
                        internship.DurationInMonths,

                    // ELIGIBILITY
                    MinimumCGPA =internship.MinimumCGPA,

                    AllowedBacklogs =
                        internship.AllowedBacklogs,

                    RequiredSkills =
                        internship.RequiredSkills,

                    AllowedDepartments =
                        internship.AllowedDepartments,

                    GraduationYear =
                        internship.GraduationYear,

                    // SEATS
                    TotalSeats =
                        internship.TotalSeats,

                    AvailableSeats =
                        internship.AvailableSeats,

                    // STATUS
                    IsActive =
                        internship.IsActive,

                    IsExpired =
                        internship.IsExpired,

                    IsSeatsFilled =
                        internship.IsSeatsFilled,

                    // DATES
                    StartDate =
                        internship.StartDate,

                    EndDate =
                        internship.EndDate,

                    RegistrationDeadline =
                        internship.RegistrationDeadline,

                    // AUDIT
                    CreatedAt =
                        internship.CreatedAt,

                    UpdatedAt =
                        internship.UpdatedAt
                });
        }
    }
}