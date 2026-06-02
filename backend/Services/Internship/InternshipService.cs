using InternshipPortal.API.Data.Context;
using InternshipPortal.API.DTOs.Internship;
using InternshipPortal.API.Entities;
using InternshipPortal.API.Helpers;
using InternshipPortal.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.API.Services.Internship
{
    public class InternshipService : IInternshipService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public InternshipService(
            ApplicationDbContext context,
            IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        private async Task CloseExpiredRegistrationInternshipsAsync()
        {
            var now = DateTime.UtcNow;
            var expired = await _context.Internships
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive &&
                    x.RegistrationDeadline.Date < now.Date)
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
                    ? null
                    : model.CoverImageUrl
            };

            if (string.IsNullOrWhiteSpace(internship.CoverImageUrl))
            {
                internship.CoverImageUrl = InternshipCoverImageHelper.GetDynamicCoverImageUrl(internship);
            }

            _context.Internships.Add(internship);

            await _context.SaveChangesAsync();

            return MapToResponseDto(internship);
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

            return MapToResponseDto(internship);
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

            var now = DateTime.UtcNow;
            var changed = false;

            if (internship.RegistrationDeadline.Date < now.Date && internship.IsActive)
            {
                internship.IsActive = false;
                internship.UpdatedAt = now;
                changed = true;
            }

            if (internship.EndDate.Date < now.Date)
            {
                internship.IsExpired = true;
                if (internship.IsActive)
                {
                    internship.IsActive = false;
                }
                internship.UpdatedAt = now;
                changed = true;
            }

            if (changed)
            {
                await _context.SaveChangesAsync();
            }

            return MapToResponseDto(internship);
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

            if (filter.OpenOnly)
            {
                var now = DateTime.UtcNow;
                query = query.Where(x =>
                    !x.IsExpired &&
                    !x.IsSeatsFilled &&
                    x.AvailableSeats > 0 &&
                    x.RegistrationDeadline.Date >= now.Date &&
                    (x.IsActive || x.RegistrationDeadline.Date >= now.Date));
            }

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

            return internships.Select(MapToResponseDto);
        }

        public async Task<InternshipResponseDto?> UploadCoverImageAsync(
            Guid internshipId,
            Guid adminId,
            IFormFile file)
        {
            var internship = await _context.Internships
                .FirstOrDefaultAsync(x =>
                    x.Id == internshipId &&
                    !x.IsDeleted);

            if (internship == null)
            {
                return null;
            }

            if (internship.AdminId != adminId)
            {
                throw new Exception("You are not authorized to update this internship");
            }

            if (!string.IsNullOrWhiteSpace(internship.CoverImageUrl) &&
                internship.CoverImageUrl.StartsWith("/cover-images/"))
            {
                _fileService.DeleteFile(internship.CoverImageUrl);
            }

            var imageUrl = await _fileService.UploadCoverImageAsync(file);
            internship.CoverImageUrl = imageUrl;
            internship.UpdatedAt = DateTime.UtcNow;
            internship.UpdatedBy = adminId;

            await _context.SaveChangesAsync();

            return MapToResponseDto(internship);
        }

        private static InternshipResponseDto MapToResponseDto(Entities.Internship internship) =>
            new()
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
                CoverImageUrl = InternshipCoverImageHelper.ResolveCoverImageUrl(internship)
            };
    }
}

