using InternshipPortal.API.Data.Context;
using InternshipPortal.API.DTOs.Application;
using InternshipPortal.API.Entities;
using InternshipPortal.API.Enums;
using InternshipPortal.API.Helpers;
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

            // CHECK INTERNSHIP OPEN FOR APPLICATIONS
            if (!EligibilityHelper.IsOpenForRegistration(internship))
            {
                throw new Exception(
                    "This internship is not open for applications"
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
            var (isEligible, ineligibilityReason) =
                EligibilityHelper.CheckStudentEligibility(profile, internship);

            if (!isEligible)
            {
                throw new Exception(
                    ineligibilityReason ?? "You are not eligible for this internship"
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

            application.Student = profile.User;
            return MapToResponseDto(application, internship);
        }

        private ApplicationResponseDto MapToResponseDto(
            Entities.Application application,
            Entities.Internship? internshipOverride = null,
            Dictionary<Guid, (int PreTestAttempts, int PostTestAttempts)>? attemptCounts = null)
        {
            var internship = internshipOverride ?? application.Internship;
            var appId = application.Id;

            var preTestAttempts = 0;
            var postTestAttempts = 0;
            if (attemptCounts != null && attemptCounts.TryGetValue(appId, out var counts))
            {
                preTestAttempts = counts.PreTestAttempts;
                postTestAttempts = counts.PostTestAttempts;
            }

            var canTakePreTest = CanTakePreTest(application, preTestAttempts);
            var canTakePostTest = CanTakeCompletionTest(application, postTestAttempts);

            return new ApplicationResponseDto
            {
                Id = application.Id,
                StudentId = application.StudentId,
                StudentName = application.Student?.FullName ?? string.Empty,
                InternshipId = application.InternshipId,
                InternshipTitle = internship?.Title ?? string.Empty,
                CompanyName = internship?.CompanyName ?? string.Empty,
                Status = application.Status.ToString(),
                DisplayStatus = ApplicationStatusHelper.GetDisplayStatus(application, preTestAttempts, postTestAttempts),
                DisplayPhase = ApplicationStatusHelper.GetDisplayPhase(application, preTestAttempts),
                AdminRemarks = application.AdminRemarks,
                AppliedAt = application.AppliedAt,
                ReviewedAt = application.ReviewedAt,
                CreatedAt = application.CreatedAt,
                UpdatedAt = application.UpdatedAt,
                IsCompleted = application.IsCompleted,
                CompletedAt = application.CompletedAt,
                CertificateUrl = application.CertificateUrl,
                IsPreTestPassed = application.IsPreTestPassed,
                PreTestScore = application.PreTestScore,
                PreTestPassedAt = application.PreTestPassedAt,
                PreTestStatus = ApplicationStatusHelper.GetPreTestStatus(application, preTestAttempts),
                PreTestAttemptsUsed = preTestAttempts,
                IsTestPassed = application.IsTestPassed,
                TestScore = application.TestScore,
                PostTestScore = application.TestScore,
                TestPassedAt = application.TestPassedAt,
                PostTestStatus = ApplicationStatusHelper.GetPostTestStatus(application, postTestAttempts),
                PostTestAttemptsUsed = postTestAttempts,
                OverallAssessmentScore = ApplicationStatusHelper.GetOverallAssessmentScore(application),
                CanTakePreTest = canTakePreTest,
                CanTakeCompletionTest = canTakePostTest,
                CanTakePostTest = canTakePostTest,
                CoverImageUrl = internship != null
                    ? InternshipCoverImageHelper.ResolveCoverImageUrl(internship)
                    : null
            };
        }

        private async Task<Dictionary<Guid, (int PreTestAttempts, int PostTestAttempts)>>
            GetAttemptCountsAsync(IEnumerable<Guid> applicationIds)
        {
            var ids = applicationIds.ToList();
            if (ids.Count == 0)
            {
                return new Dictionary<Guid, (int, int)>();
            }

            try
            {
                var sessions = await _context.InternshipTestSessions
                    .Where(x => ids.Contains(x.ApplicationId) && !x.IsDeleted)
                    .Select(x => new { x.ApplicationId, x.TestType })
                    .ToListAsync();

                return sessions
                    .GroupBy(x => x.ApplicationId)
                    .ToDictionary(
                        g => g.Key,
                        g => (
                            PreTestAttempts: g.Count(x => x.TestType == TestType.PreTest),
                            PostTestAttempts: g.Count(x => x.TestType == TestType.CompletionTest)
                        ));
            }
            catch
            {
                return ids.ToDictionary(id => id, _ => (0, 0));
            }
        }

        private static bool CanTakePreTest(Entities.Application application, int preTestAttemptsUsed)
        {
            if (application.IsCompleted || application.IsPreTestPassed) return false;
            if (application.Status != ApplicationStatus.Accepted) return false;
            return preTestAttemptsUsed < 1;
        }

        private static bool CanTakeCompletionTest(Entities.Application application, int postTestAttemptsUsed)
        {
            if (application.IsCompleted || application.IsTestPassed) return false;
            if (!application.IsPreTestPassed) return false;
            if (application.Status != ApplicationStatus.InProgress) return false;
            return postTestAttemptsUsed < 5;
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

            var attemptCounts = await GetAttemptCountsAsync(applications.Select(a => a.Id));
            return applications.Select(a => MapToResponseDto(a, attemptCounts: attemptCounts));
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

            var attemptCounts = await GetAttemptCountsAsync(applications.Select(a => a.Id));
            return applications.Select(a => MapToResponseDto(a, attemptCounts: attemptCounts));
        }

        public async Task<StudentAssessmentDto?> GetStudentAssessmentAsync(
            Guid applicationId,
            Guid adminId)
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

            if (application.Internship.AdminId != adminId)
            {
                throw new Exception("You are not authorized to view this assessment");
            }

            var attemptCounts = await GetAttemptCountsAsync(new[] { application.Id });
            var preTestAttempts = attemptCounts.TryGetValue(application.Id, out var counts)
                ? counts.PreTestAttempts
                : 0;
            var postTestAttempts = attemptCounts.TryGetValue(application.Id, out counts)
                ? counts.PostTestAttempts
                : 0;

            var mapped = MapToResponseDto(application, attemptCounts: attemptCounts);

            return new StudentAssessmentDto
            {
                ApplicationId = mapped.Id,
                StudentId = mapped.StudentId,
                StudentName = mapped.StudentName,
                StudentEmail = application.Student.Email ?? string.Empty,
                InternshipId = mapped.InternshipId,
                InternshipTitle = mapped.InternshipTitle,
                Status = mapped.Status,
                DisplayStatus = mapped.DisplayStatus,
                DisplayPhase = mapped.DisplayPhase,
                PreTestStatus = mapped.PreTestStatus,
                PreTestScore = mapped.PreTestScore,
                PreTestPassedAt = mapped.PreTestPassedAt,
                PreTestAttemptsUsed = preTestAttempts,
                PostTestStatus = mapped.PostTestStatus,
                PostTestScore = mapped.PostTestScore,
                PostTestPassedAt = mapped.TestPassedAt,
                PostTestAttemptsUsed = postTestAttempts,
                OverallAssessmentScore = mapped.OverallAssessmentScore,
                CanTakePreTest = mapped.CanTakePreTest,
                CanTakePostTest = mapped.CanTakePostTest
            };
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
            var preTestSection = parsedStatus == ApplicationStatus.Accepted
                ? $@"

IMPORTANT — PRE-TEST REQUIRED:
Your application has been accepted! You must complete a one-time pre-test (10 questions on algorithms, data structures, and complexity) with a minimum score of 70% to begin your internship.

Log in to your Student Portal → My Applications → Take Pre-Test.

You get only ONE attempt, so prepare well before starting."
                : string.Empty;

            var body = $@"
Hello {application.Student.FullName},

Your application status for:

{application.Internship.Title}

has been updated.

NEW STATUS:
{application.Status}

ADMIN REMARKS:
{application.AdminRemarks}
{preTestSection}

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

            if (parsedStatus == ApplicationStatus.Accepted)
            {
                await _notificationService.CreateNotificationAsync(
                    application.StudentId,
                    "Pre-test Required",
                    $"Complete your one-time pre-test for '{application.Internship.Title}' to start your internship. Go to My Applications.",
                    "PreTestRequired"
                );
            }

            return await MapSingleApplicationAsync(application);
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

            // MUST HAVE PASSED PRE-TEST AND COMPLETION TEST
            if (!application.IsPreTestPassed)
            {
                throw new Exception(
                    "Student must pass the pre-test before completing the internship"
                );
            }

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

            return await MapSingleApplicationAsync(application);
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
                CoverImageUrl = InternshipCoverImageHelper.ResolveCoverImageUrl(application.Internship),
                IsCompleted = application.IsCompleted
            };
        }

        private async Task<ApplicationResponseDto> MapSingleApplicationAsync(Entities.Application application)
        {
            var attemptCounts = await GetAttemptCountsAsync(new[] { application.Id });
            return MapToResponseDto(application, attemptCounts: attemptCounts);
        }
    }
}