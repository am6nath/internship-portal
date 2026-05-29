using InternshipPortal.API.Data.Context;
using InternshipPortal.API.DTOs.Application;
using InternshipPortal.API.Entities;
using InternshipPortal.API.Enums;
using InternshipPortal.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.API.Services.Application
{
    public class ApplicationService
        : IApplicationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public ApplicationService(
            ApplicationDbContext context,
            IEmailService emailService,
            INotificationService notificationService)
        {
            _context = context;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        // APPLY INTERNSHIP
        public async Task<ApplicationResponseDto>
            ApplyInternshipAsync(
                Guid studentId,
                ApplyInternshipDto model)
        {
            // CHECK STUDENT PROFILE
            var profile = await _context.StudentProfiles
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.UserId == studentId &&
                    !x.IsDeleted);

            if (profile == null)
            {
                throw new Exception(
                    "Complete your student profile first"
                );
            }

            // CHECK INTERNSHIP
            var internship = await _context.Internships
                .Include(x => x.Admin)
                .FirstOrDefaultAsync(x =>
                    x.Id == model.InternshipId &&
                    !x.IsDeleted);

            if (internship == null)
            {
                throw new Exception(
                    "Internship not found"
                );
            }

            // CHECK ACTIVE
            if (!internship.IsActive ||
                internship.IsExpired)
            {
                throw new Exception(
                    "Internship is not active"
                );
            }

            // CHECK DEADLINE
            if (internship.RegistrationDeadline
                < DateTime.UtcNow)
            {
                throw new Exception(
                    "Registration deadline has passed"
                );
            }

            // CHECK SEATS
            if (internship.AvailableSeats <= 0)
            {
                internship.IsSeatsFilled = true;

                await _context.SaveChangesAsync();


                throw new Exception(
                    "No seats available"
                );
            }


            // DUPLICATE CHECK
            var existingApplication =
                await _context.Applications
                    .FirstOrDefaultAsync(x =>
                        x.StudentId == studentId &&
                        x.InternshipId ==
                            model.InternshipId &&
                        !x.IsDeleted);

            if (existingApplication != null)
            {
                throw new Exception(
                    "You already applied for this internship"
                );
            }

            // ELIGIBILITY CHECK
            if (profile.CGPA <
                internship.MinimumCGPA)
            {
                throw new Exception(
                    "CGPA criteria not matched"
                );
            }

            if (profile.Backlogs >
                internship.AllowedBacklogs)
            {
                throw new Exception(
                    "Backlog criteria not matched"
                );
            }

            if (!internship.AllowedDepartments
                .Contains(profile.Department))
            {
                throw new Exception(
                    "Department not eligible"
                );
            }

            if (profile.GraduationYear !=
                internship.GraduationYear)
            {
                throw new Exception(
                    "Graduation year not eligible"
                );
            }

            // CREATE APPLICATION
            var application = new Entities.Application
            {
                StudentId = studentId,

                InternshipId =
                    model.InternshipId,

                Status = ApplicationStatus.Pending,
                AppliedAt = DateTime.UtcNow,

                CreatedBy = studentId
            };

            _context.Applications.Add(application);

            await _context.SaveChangesAsync();

            return new ApplicationResponseDto
            {
                Id = application.Id,

                StudentId = studentId,

                StudentName =
                    profile.User.FullName,

                InternshipId =
                    internship.Id,

                InternshipTitle =
                    internship.Title,

                CompanyName =
                    internship.CompanyName,

                Status = application.Status.ToString(),

                AdminRemarks =
                    application.AdminRemarks,

                AppliedAt =
                    application.AppliedAt,

                ReviewedAt =
                    application.ReviewedAt,

                CreatedAt =
                    application.CreatedAt,

                UpdatedAt =
                    application.UpdatedAt
            };
        }

        // GET STUDENT APPLICATIONS
        public async Task<IEnumerable<ApplicationResponseDto>> GetStudentApplicationsAsync(Guid studentId)
        {
            var applications = await _context.Applications
                .Include(x => x.Internship)
                .Include(x => x.Student)
                .Where(x => x.StudentId == studentId && !x.IsDeleted)
                .OrderByDescending(x => x.AppliedAt)
                .ToListAsync();

            return applications.Select(application => new ApplicationResponseDto
            {
                Id = application.Id,
                StudentId = application.StudentId,
                StudentName = application.Student.FullName,
                InternshipId = application.InternshipId,
                InternshipTitle = application.Internship.Title,
                CompanyName = application.Internship.CompanyName,
                Status = application.Status.ToString(),
                AdminRemarks = application.AdminRemarks,
                AppliedAt = application.AppliedAt,
                ReviewedAt = application.ReviewedAt,
                CreatedAt = application.CreatedAt,
                UpdatedAt = application.UpdatedAt,
                IsCompleted = application.IsCompleted,
                CompletedAt = application.CompletedAt,
                CertificateUrl = application.CertificateUrl,
                IsTestPassed = application.IsTestPassed,
                TestScore = application.TestScore,
                TestPassedAt = application.TestPassedAt
            });
        }

        // GET INTERNSHIP APPLICATIONS
        public async Task<IEnumerable<ApplicationResponseDto>> GetInternshipApplicationsAsync(Guid internshipId)
        {
            var applications = await _context.Applications
                .Include(x => x.Student)
                .Include(x => x.Internship)
                .Where(x => x.InternshipId == internshipId && !x.IsDeleted)
                .OrderByDescending(x => x.AppliedAt)
                .ToListAsync();

            return applications.Select(application => new ApplicationResponseDto
            {
                Id = application.Id,
                StudentId = application.StudentId,
                StudentName = application.Student.FullName,
                InternshipId = application.InternshipId,
                InternshipTitle = application.Internship.Title,
                CompanyName = application.Internship.CompanyName,
                Status = application.Status.ToString(),
                AdminRemarks = application.AdminRemarks,
                AppliedAt = application.AppliedAt,
                ReviewedAt = application.ReviewedAt,
                CreatedAt = application.CreatedAt,
                UpdatedAt = application.UpdatedAt,
                IsCompleted = application.IsCompleted,
                CompletedAt = application.CompletedAt,
                CertificateUrl = application.CertificateUrl,
                IsTestPassed = application.IsTestPassed,
                TestScore = application.TestScore,
                TestPassedAt = application.TestPassedAt
            });
        }

        // UPDATE APPLICATION STATUS
        public async Task<ApplicationResponseDto?>
            UpdateApplicationStatusAsync(
                Guid applicationId,
                Guid adminId,
                UpdateApplicationStatusDto model)
        {
            var application = await _context.Applications
                .Include(x => x.Student)
                .Include(x => x.Internship)
                .FirstOrDefaultAsync(x =>
                    x.Id == applicationId &&
                    !x.IsDeleted);

            if (application == null)
            {
                return null;
            }

            // CHECK INTERNSHIP OWNER
            if (application.Internship.AdminId
                != adminId)
            {
                throw new Exception(
                    "You are not authorized to review this application"
                );
            }

            // PREVENT MULTIPLE ACCEPT
            if (application.Status ==
                ApplicationStatus.Accepted)
            {
                throw new Exception(
                    "Application already accepted"
                );
            }

            // UPDATE STATUS
            if (!Enum.TryParse<ApplicationStatus>(
                    model.Status,
                    true,
                    out var parsedStatus))
            {
                throw new Exception(
                    "Invalid application status"
                );
            }

            application.Status = parsedStatus;
            application.AdminRemarks =
                model.AdminRemarks;

            application.ReviewedAt =
                DateTime.UtcNow;

            application.UpdatedAt =
                DateTime.UtcNow;

            application.UpdatedBy =
                adminId;

            // REDUCE SEAT WHEN ACCEPTED
            if (parsedStatus ==
                ApplicationStatus.Accepted)
            {
                if (application.Internship
                    .AvailableSeats <= 0)
                {
                    application.Internship
                        .IsSeatsFilled = true;

                    throw new Exception(
                        "No seats available"
                    );
                }

                application.Internship
                    .AvailableSeats--;

                // AUTO SEAT FILLED
                if (application.Internship
                    .AvailableSeats == 0)
                {
                    application.Internship
                        .IsSeatsFilled = true;

                    application.Internship
                        .IsActive = false;
                }
            }

            await _context.SaveChangesAsync();

            // EMAIL SUBJECT
            var subject =
                $"Application Status Updated - {application.Internship.Title}";

            // EMAIL BODY
            var body = $@"
Hello {application.Student.FullName},

Your application status for:

{application.Internship.Title}

has been updated.

NEW STATUS:
{application.Status}

ADMIN REMARKS:
{application.AdminRemarks}

Thank you,
Internship Portal
";

            // SEND EMAIL
            await _emailService.SendEmailAsync(
                application.Student.Email!,
                subject,
                body
            );

            // IN-APP NOTIFICATION
            await _notificationService.CreateNotificationAsync(
                application.StudentId,
                $"Application {application.Status}",
                $"Your application for '{application.Internship.Title}' has been updated to '{application.Status}'.",
                "ApplicationStatus"
            );

            return new ApplicationResponseDto
            {
                Id = application.Id,

                // STUDENT
                StudentId =
                    application.StudentId,

                StudentName =
                    application.Student.FullName,

                // INTERNSHIP
                InternshipId =
                    application.InternshipId,

                InternshipTitle =
                    application.Internship.Title,

                CompanyName =
                    application.Internship.CompanyName,

                // STATUS
                Status =
                    application.Status.ToString(),

                AdminRemarks =
                    application.AdminRemarks,

                // DATES
                AppliedAt =
                    application.AppliedAt,

                ReviewedAt =
                    application.ReviewedAt,

                // AUDIT
                CreatedAt =
                    application.CreatedAt,

                UpdatedAt =
                    application.UpdatedAt,

                IsCompleted = application.IsCompleted,
                CompletedAt = application.CompletedAt,
                CertificateUrl = application.CertificateUrl,
                IsTestPassed = application.IsTestPassed,
                TestScore = application.TestScore,
                TestPassedAt = application.TestPassedAt
            };
        }

        // COMPLETE INTERNSHIP
        public async Task<ApplicationResponseDto?>
            CompleteInternshipAsync(
                Guid applicationId,
                Guid adminId,
                CompleteInternshipDto model)
        {
            var application =
                await _context.Applications
                    .Include(x => x.Student)
                    .Include(x => x.Internship)
                    .FirstOrDefaultAsync(x =>
                        x.Id == applicationId &&
                        !x.IsDeleted);

            if (application == null)
            {
                return null;
            }

            // ADMIN OWNERSHIP CHECK
            if (application.Internship.AdminId
                != adminId)
            {
                throw new Exception(
                    "Unauthorized action"
                );
            }

            // MUST HAVE PASSED COMPLETION TEST
            if (!application.IsTestPassed)
            {
                throw new Exception(
                    "Student must pass the completion test (70% minimum) before issuing a certificate"
                );
            }

            // MUST BE ACCEPTED OR IN PROGRESS
            if (application.Status != ApplicationStatus.Accepted &&
                application.Status != ApplicationStatus.InProgress)
            {
                throw new Exception(
                    "Only accepted or in-progress internships can be completed"
                );
            }

            // MARK COMPLETED
            application.Status =
                ApplicationStatus.Completed;

            application.IsCompleted = true;

            application.CompletedAt =
                DateTime.UtcNow;

            application.CertificateUrl = string.IsNullOrWhiteSpace(model.CertificateUrl)
                ? $"http://localhost:4200/verify-certificate/{application.Id}"
                : model.CertificateUrl;

            application.UpdatedAt =
                DateTime.UtcNow;

            application.UpdatedBy =
                adminId;

            await _context.SaveChangesAsync();

            // EMAIL
            var subject =
                $"Internship Completed - {application.Internship.Title}";

            var body = $@"
Dear {application.Student.FullName},

Congratulations! You have successfully completed your internship for '{application.Internship.Title}' at {application.Internship.CompanyName}.

Your hard work and dedication have earned you your official completion certificate!

Verify ID: CERT-{application.Id.ToString().Substring(0, 8).ToUpper()}-{application.CompletedAt?.Year}
Verification URL: {application.CertificateUrl}

You can view, download/print your high-resolution certificate or directly share it on LinkedIn from your Student Portal.

Best regards,
Internship Portal Team
";

            await _emailService.SendEmailAsync(
                application.Student.Email!,
                subject,
                body
            );

            // IN-APP NOTIFICATION
            await _notificationService.CreateNotificationAsync(
                application.StudentId,
                "Internship Completed",
                $"Congratulations! You have successfully completed your internship for '{application.Internship.Title}'.",
                "ApplicationStatus"
            );

            return new ApplicationResponseDto
            {
                Id = application.Id,

                StudentId =
                    application.StudentId,

                StudentName =
                    application.Student.FullName,

                InternshipId =
                    application.InternshipId,

                InternshipTitle =
                    application.Internship.Title,

                CompanyName =
                    application.Internship.CompanyName,

                Status =
                    application.Status.ToString(),

                AdminRemarks =
                    application.AdminRemarks,

                AppliedAt =
                    application.AppliedAt,

                ReviewedAt =
                    application.ReviewedAt,

                CreatedAt =
                    application.CreatedAt,

                UpdatedAt =
                    application.UpdatedAt,

                IsCompleted =
                    application.IsCompleted,

                CompletedAt =
                    application.CompletedAt,

                CertificateUrl =
                    application.CertificateUrl,

                IsTestPassed = application.IsTestPassed,
                TestScore = application.TestScore,
                TestPassedAt = application.TestPassedAt
            };
        }

        public async Task<CertificateDetailsDto?>
            GetCertificateDetailsAsync(Guid applicationId)
        {
            var application = await _context.Applications
                .Include(x => x.Student)
                .Include(x => x.Internship)
                .FirstOrDefaultAsync(x =>
                    x.Id == applicationId &&
                    !x.IsDeleted);

            if (application == null || !application.IsCompleted)
            {
                return null;
            }

            var code = $"CERT-{application.Id.ToString().Substring(0, 8).ToUpper()}-{application.CompletedAt?.Year ?? DateTime.UtcNow.Year}";

            return new CertificateDetailsDto
            {
                ApplicationId = application.Id,
                StudentName = application.Student.FullName,
                InternshipTitle = application.Internship.Title,
                CompanyName = application.Internship.CompanyName,
                DurationInMonths = application.Internship.DurationInMonths,
                StartDate = application.Internship.StartDate,
                EndDate = application.Internship.EndDate,
                CompletedAt = application.CompletedAt ?? DateTime.UtcNow,
                VerificationCode = code,
                CoverImageUrl = application.Internship.CoverImageUrl,
                IsCompleted = application.IsCompleted
            };
        }
    }
}