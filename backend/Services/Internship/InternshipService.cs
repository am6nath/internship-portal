using InternshipPortal.API.Data.Context;
using InternshipPortal.API.DTOs.Internship;
using InternshipPortal.API.Entities;
using InternshipPortal.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.API.Services.Internship
{
    public class InternshipService : IInternshipService
    {
        private readonly ApplicationDbContext _context;

        public InternshipService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task CloseExpiredRegistrationInternshipsAsync()
        {
            var now = DateTime.UtcNow;
            var expired = await _context.Internships
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive &&
                    x.RegistrationDeadline < now)
                .ToListAsync();

            if (expired.Count == 0)
            {
                return;
            }

            foreach (var internship in expired)
            {
                internship.IsActive = false;
                internship.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();
        }

        // CREATE INTERNSHIP
        public async Task<InternshipResponseDto>
            CreateInternshipAsync(
                Guid adminId,
                CreateInternshipDto model)
        {
            var internship = new Entities.Internship
            {
                Title = model.Title,
                Description = model.Description,
                CompanyName = model.CompanyName,
                Location = model.Location,
                Stipend = model.Stipend,
                DurationInMonths = model.DurationInMonths,

                // ELIGIBILITY
                MinimumCGPA = model.MinimumCGPA,
                AllowedBacklogs = model.AllowedBacklogs,
                RequiredSkills = model.RequiredSkills,
                AllowedDepartments = model.AllowedDepartments,
                GraduationYear = model.GraduationYear,

                // SEATS
                TotalSeats = model.TotalSeats,
                AvailableSeats = model.TotalSeats,

                // DATES
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                RegistrationDeadline = model.RegistrationDeadline,

                // STATUS
                IsActive = true,
                IsExpired = false,
                IsSeatsFilled = false,

                // ADMIN
                AdminId = adminId,

                CreatedBy = adminId,

                CoverImageUrl = string.IsNullOrWhiteSpace(model.CoverImageUrl)
                    ? "https://images.unsplash.com/photo-1555066931-4365d14bab8c?w=1200"
                    : model.CoverImageUrl
            };

            _context.Internships.Add(internship);

            await _context.SaveChangesAsync();

            return new InternshipResponseDto
            {
                Id = internship.Id,

                Title = internship.Title,
                Description = internship.Description,
                CompanyName = internship.CompanyName,
                Location = internship.Location,
                Stipend = internship.Stipend,
                DurationInMonths = internship.DurationInMonths,

                // ELIGIBILITY
                MinimumCGPA = internship.MinimumCGPA,
                AllowedBacklogs = internship.AllowedBacklogs,
                RequiredSkills = internship.RequiredSkills,
                AllowedDepartments = internship.AllowedDepartments,
                GraduationYear = internship.GraduationYear,

                // SEATS
                TotalSeats = internship.TotalSeats,
                AvailableSeats = internship.AvailableSeats,

                // STATUS
                IsActive = internship.IsActive,
                IsExpired = internship.IsExpired,
                IsSeatsFilled = internship.IsSeatsFilled,

                // DATES
                StartDate = internship.StartDate,
                EndDate = internship.EndDate,
                RegistrationDeadline = internship.RegistrationDeadline,

                // AUDIT
                CreatedAt = internship.CreatedAt,
                UpdatedAt = internship.UpdatedAt,
                CoverImageUrl = internship.CoverImageUrl
            };
        }
        // UPDATE INTERNSHIP
        public async Task<InternshipResponseDto?>
            UpdateInternshipAsync(
                Guid internshipId,
                Guid adminId,
                UpdateInternshipDto model)
        {
            var internship = await _context.Internships
                .FirstOrDefaultAsync(x =>
                    x.Id == internshipId &&
                    !x.IsDeleted);

            if (internship == null)
            {
                return null;
            }

            // OWNERSHIP SECURITY
            if (internship.AdminId != adminId)
            {
                throw new Exception(
                    "You are not authorized to update this internship"
                );
            }

            // UPDATE FIELDS
            internship.Title = model.Title;
            internship.Description = model.Description;
            internship.CompanyName = model.CompanyName;
            internship.Location = model.Location;
            internship.Stipend = model.Stipend;
            internship.DurationInMonths = model.DurationInMonths;

            // ELIGIBILITY
            internship.MinimumCGPA = model.MinimumCGPA;
            internship.AllowedBacklogs = model.AllowedBacklogs;
            internship.RequiredSkills = model.RequiredSkills;
            internship.AllowedDepartments = model.AllowedDepartments;
            internship.GraduationYear = model.GraduationYear;

            // SEATS
            internship.TotalSeats = model.TotalSeats;

            // DATES
            internship.StartDate = model.StartDate;
            internship.EndDate = model.EndDate;
            internship.RegistrationDeadline =
                model.RegistrationDeadline;

            // AUDIT
            internship.UpdatedAt = DateTime.UtcNow;
            internship.UpdatedBy = adminId;
            internship.CoverImageUrl = string.IsNullOrWhiteSpace(model.CoverImageUrl)
                ? internship.CoverImageUrl
                : model.CoverImageUrl;

            await _context.SaveChangesAsync();

            return new InternshipResponseDto
            {
                Id = internship.Id,

                Title = internship.Title,
                Description = internship.Description,
                CompanyName = internship.CompanyName,
                Location = internship.Location,
                Stipend = internship.Stipend,
                DurationInMonths = internship.DurationInMonths,

                // ELIGIBILITY
                MinimumCGPA = internship.MinimumCGPA,
                AllowedBacklogs = internship.AllowedBacklogs,
                RequiredSkills = internship.RequiredSkills,
                AllowedDepartments = internship.AllowedDepartments,
                GraduationYear = internship.GraduationYear,

                // SEATS
                TotalSeats = internship.TotalSeats,
                AvailableSeats = internship.AvailableSeats,

                // STATUS
                IsActive = internship.IsActive,
                IsExpired = internship.IsExpired,
                IsSeatsFilled = internship.IsSeatsFilled,

                // DATES
                StartDate = internship.StartDate,
                EndDate = internship.EndDate,
                RegistrationDeadline = internship.RegistrationDeadline,

                // AUDIT
                CreatedAt = internship.CreatedAt,
                UpdatedAt = internship.UpdatedAt,
                CoverImageUrl = internship.CoverImageUrl
            };
        }
        // DELETE INTERNSHIP (SOFT DELETE)
        public async Task<bool> DeleteInternshipAsync(
            Guid internshipId,
            Guid adminId)
        {
            var internship = await _context.Internships
                .FirstOrDefaultAsync(x =>
                    x.Id == internshipId &&
                    !x.IsDeleted);

            if (internship == null)
            {
                return false;
            }

            // OWNERSHIP SECURITY
            if (internship.AdminId != adminId)
            {
                throw new Exception(
                    "You are not authorized to delete this internship"
                );
            }

            // SOFT DELETE
            internship.IsDeleted = true;

            internship.IsActive = false;

            internship.UpdatedAt = DateTime.UtcNow;

            internship.UpdatedBy = adminId;

            await _context.SaveChangesAsync();

            return true;
        }
        // GET INTERNSHIP BY ID
        public async Task<InternshipResponseDto?>
            GetInternshipByIdAsync(Guid internshipId)
        {
            var internship = await _context.Internships
                .FirstOrDefaultAsync(x =>
                    x.Id == internshipId &&
                    !x.IsDeleted);

            if (internship == null)
            {
                return null;
            }

            if (internship.RegistrationDeadline < DateTime.UtcNow && internship.IsActive)
            {
                internship.IsActive = false;
                internship.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            // CHECK EXPIRY
            if (internship.EndDate < DateTime.UtcNow)
            {
                internship.IsExpired = true;
                internship.IsActive = false;

                await _context.SaveChangesAsync();
            }

            return new InternshipResponseDto
            {
                Id = internship.Id,

                Title = internship.Title,
                Description = internship.Description,
                CompanyName = internship.CompanyName,
                Location = internship.Location,
                Stipend = internship.Stipend,
                DurationInMonths = internship.DurationInMonths,

                // ELIGIBILITY
                MinimumCGPA = internship.MinimumCGPA,
                AllowedBacklogs = internship.AllowedBacklogs,
                RequiredSkills = internship.RequiredSkills,
                AllowedDepartments = internship.AllowedDepartments,
                GraduationYear = internship.GraduationYear,

                // SEATS
                TotalSeats = internship.TotalSeats,
                AvailableSeats = internship.AvailableSeats,

                // STATUS
                IsActive = internship.IsActive,
                IsExpired = internship.IsExpired,
                IsSeatsFilled = internship.IsSeatsFilled,

                // DATES
                StartDate = internship.StartDate,
                EndDate = internship.EndDate,
                RegistrationDeadline = internship.RegistrationDeadline,

                // AUDIT
                CreatedAt = internship.CreatedAt,
                UpdatedAt = internship.UpdatedAt,
                CoverImageUrl = internship.CoverImageUrl
            };
        }
        // GET ALL INTERNSHIPS
        public async Task<IEnumerable<InternshipResponseDto>>
            GetAllInternshipsAsync(
                InternshipFilterDto filter)
        {
            await CloseExpiredRegistrationInternshipsAsync();

            var query = _context.Internships
                .Where(x =>
                    !x.IsDeleted)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(x =>
                    x.Title.Contains(filter.Search) ||
                    x.CompanyName.Contains(filter.Search) ||
                    x.Location.Contains(filter.Search));
            }

            // DEPARTMENT FILTER
            if (!string.IsNullOrWhiteSpace(filter.Department))
            {
                query = query.Where(x =>
                    x.AllowedDepartments.Contains(
                        filter.Department));
            }
            // COMPANY FILTER
            if (!string.IsNullOrWhiteSpace(
                    filter.CompanyName))
            {
                query = query.Where(x =>
                    x.CompanyName.Contains(
                        filter.CompanyName));
            }
            // LOCATION FILTER
            if (!string.IsNullOrWhiteSpace(
                    filter.Location))
            {
                query = query.Where(x =>
                    x.Location.Contains(
                        filter.Location));
            }




            // CGPA FILTER
            if (filter.MinimumCGPA.HasValue)
            {
                query = query.Where(x =>
                    x.MinimumCGPA <=
                    filter.MinimumCGPA.Value);
            }

            // SORTING
            query =
                filter.SortBy?.ToLower() switch
                {
                    "deadline" =>
                        filter.Descending
                            ? query.OrderByDescending(x =>
                                x.RegistrationDeadline)
                            : query.OrderBy(x =>
                                x.RegistrationDeadline),

                    "stipend" =>
                        filter.Descending
                            ? query.OrderByDescending(x =>
                                x.Stipend)
                            : query.OrderBy(x =>
                                x.Stipend),

                    "company" =>
                        filter.Descending
                            ? query.OrderByDescending(x =>
                                x.CompanyName)
                            : query.OrderBy(x =>
                                x.CompanyName),

                    _ =>
                        query.OrderByDescending(x =>
                            x.CreatedAt)
                };


            // PAGINATION
            query = query
                .Skip((filter.PageNumber - 1)
                    * filter.PageSize)
                .Take(filter.PageSize);

            var internships = await query.ToListAsync();

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
                    MinimumCGPA =
                        internship.MinimumCGPA,

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
                        internship.UpdatedAt,

                    CoverImageUrl =
                        internship.CoverImageUrl
                });
        }
    }
}

