using InternshipPortal.API.Data.Context;
using InternshipPortal.API.DTOs.Feedback;
using InternshipPortal.API.Entities;
using InternshipPortal.API.Enums;
using InternshipPortal.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.API.Services.Feedback
{
    public class FeedbackService : IFeedbackService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public FeedbackService(ApplicationDbContext context, IEmailService emailService, INotificationService notificationService)
        {
            _context = context;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        private async Task ReleaseFormAsync(FeedbackForm feedbackForm)
        {
            feedbackForm.IsReleased = true;

            var internship = await _context.Internships
                .Include(x => x.Applications)
                .FirstOrDefaultAsync(x => x.Id == feedbackForm.InternshipId && !x.IsDeleted);

            if (internship != null)
            {
                var allowedStatusesList = (feedbackForm.AllowedStatuses ?? "Completed")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToList();

                var targetApplications = internship.Applications
                    .Where(x => !x.IsDeleted)
                    .ToList()
                    .Where(x => allowedStatusesList.Contains(x.Status.ToString()))
                    .ToList();

                foreach (var application in targetApplications)
                {
                    var student = await _context.Users
                        .FirstOrDefaultAsync(x => x.Id == application.StudentId);

                    if (student == null) continue;

                    var subject = $"Feedback Form Released - {internship.Title}";
                    var body = $@"Hello {student.FullName},

A feedback form has been released for your internship.

Internship:
{internship.Title}

Deadline:
{feedbackForm.SubmissionDeadline}

Please submit your feedback before the deadline.

Thank you,
Internship Portal";

                    await _emailService.SendEmailAsync(student.Email!, subject, body);

                    await _notificationService.CreateNotificationAsync(
                        student.Id,
                        "Feedback Form Released",
                        $"A new feedback form '{feedbackForm.Title}' has been released for your internship '{internship.Title}'.",
                        "Feedback"
                    );
                }
            }
        }

        public async Task CreateFeedbackFormAsync(Guid adminId, CreateFeedbackFormDto model)
        {
            var internship = await _context.Internships
                .Include(x => x.Applications)
                .FirstOrDefaultAsync(x => x.Id == model.InternshipId && !x.IsDeleted);

            if (internship == null)
            {
                throw new Exception("Internship not found");
            }

            var allowedStatusesStr = model.AllowedStatuses != null && model.AllowedStatuses.Any()
                ? string.Join(",", model.AllowedStatuses)
                : "Completed";

            var feedbackForm = new FeedbackForm
            {
                InternshipId = model.InternshipId,
                Title = model.Title,
                Description = model.Description,
                ReleaseDate = model.ReleaseDate,
                SubmissionDeadline = model.SubmissionDeadline,
                AllowAnonymous = model.AllowAnonymous,
                MaxRating = model.MaxRating,
                AllowedStatuses = allowedStatusesStr,
                IsReleased = false,
                IsClosed = false,
                CreatedBy = adminId
            };

            _context.FeedbackForms.Add(feedbackForm);
            await _context.SaveChangesAsync();

            // AUTO RELEASE
            if (model.ReleaseDate <= DateTime.UtcNow)
            {
                await ReleaseFormAsync(feedbackForm);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SubmitFeedbackAsync(Guid studentId, SubmitFeedbackDto model)
        {
            var feedbackForm = await _context.FeedbackForms
                .Include(x => x.Internship)
                .FirstOrDefaultAsync(x => x.Id == model.FeedbackFormId && !x.IsDeleted);

            if (feedbackForm == null)
            {
                throw new Exception("Feedback form not found");
            }

            if (!feedbackForm.IsReleased && feedbackForm.ReleaseDate <= DateTime.UtcNow)
            {
                await ReleaseFormAsync(feedbackForm);
                await _context.SaveChangesAsync();
            }

            if (!feedbackForm.IsReleased)
            {
                throw new Exception("Feedback form not released yet");
            }

            if (feedbackForm.IsClosed)
            {
                throw new Exception("Feedback form already closed");
            }

            if (feedbackForm.SubmissionDeadline < DateTime.UtcNow)
            {
                feedbackForm.IsClosed = true;
                await _context.SaveChangesAsync();
                throw new Exception("Feedback submission deadline expired");
            }

            var application = await _context.Applications
                .FirstOrDefaultAsync(x => x.Id == model.ApplicationId && x.StudentId == studentId && !x.IsDeleted);

            if (application == null)
            {
                throw new Exception("Application not found");
            }

            var allowedStatusesList = (feedbackForm.AllowedStatuses ?? "Completed")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToList();

            if (!allowedStatusesList.Contains(application.Status.ToString()))
            {
                throw new Exception($"Your current status '{application.Status}' is not allowed to submit this feedback form");
            }

            var existingFeedback = await _context.FeedbackSubmissions
                .FirstOrDefaultAsync(x => x.ApplicationId == model.ApplicationId && x.StudentId == studentId && !x.IsDeleted);

            if (existingFeedback != null)
            {
                throw new Exception("Feedback already submitted");
            }

            if (model.Rating < 1 || model.Rating > feedbackForm.MaxRating)
            {
                throw new Exception($"Rating must be between 1 and {feedbackForm.MaxRating}");
            }

            var feedback = new FeedbackSubmission
            {
                FeedbackFormId = model.FeedbackFormId,
                StudentId = studentId,
                ApplicationId = model.ApplicationId,
                Rating = model.Rating,
                Comments = model.Comments,
                IsAnonymous = model.IsAnonymous,
                SubmittedAt = DateTime.UtcNow,
                CreatedBy = studentId
            };

            _context.FeedbackSubmissions.Add(feedback);
            await _context.SaveChangesAsync();
        }

        // GET INTERNSHIP FEEDBACKS
        public async Task<IEnumerable<FeedbackResponseDto>> GetInternshipFeedbacksAsync(Guid internshipId)
        {
            var feedbacks = await _context.FeedbackSubmissions
                .Include(x => x.Student)
                .Include(x => x.FeedbackForm)
                    .ThenInclude(x => x.Internship)
                .Where(x => x.FeedbackForm.InternshipId == internshipId && !x.IsDeleted)
                .OrderByDescending(x => x.SubmittedAt)
                .ToListAsync();

            return feedbacks.Select(x => new FeedbackResponseDto
            {
                Id = x.Id,
                StudentId = x.StudentId,
                StudentName = x.IsAnonymous ? "Anonymous" : x.Student.FullName,
                InternshipId = x.FeedbackForm.InternshipId,
                InternshipTitle = x.FeedbackForm.Internship.Title,
                Rating = x.Rating,
                Comments = x.Comments,
                IsAnonymous = x.IsAnonymous,
                SubmittedAt = x.SubmittedAt,
                MaxRating = x.FeedbackForm.MaxRating
            });
        }

        // GET STUDENT FEEDBACKS
        public async Task<IEnumerable<FeedbackResponseDto>> GetStudentFeedbacksAsync(Guid studentId)
        {
            var feedbacks = await _context.FeedbackSubmissions
                .Include(x => x.Student)
                .Include(x => x.FeedbackForm)
                    .ThenInclude(x => x.Internship)
                .Where(x => x.StudentId == studentId && !x.IsDeleted)
                .OrderByDescending(x => x.SubmittedAt)
                .ToListAsync();

            return feedbacks.Select(x => new FeedbackResponseDto
            {
                Id = x.Id,
                StudentId = x.StudentId,
                StudentName = x.Student.FullName,
                InternshipId = x.FeedbackForm.InternshipId,
                InternshipTitle = x.FeedbackForm.Internship.Title,
                Rating = x.Rating,
                Comments = x.Comments,
                IsAnonymous = x.IsAnonymous,
                SubmittedAt = x.SubmittedAt,
                MaxRating = x.FeedbackForm.MaxRating
            });
        }

        // GET FEEDBACK FORMS FOR INTERNSHIP
        public async Task<IEnumerable<FeedbackFormDto>> GetFeedbackFormsAsync(Guid internshipId)
        {
            var forms = await _context.FeedbackForms
                .Where(x => x.InternshipId == internshipId && !x.IsDeleted)
                .OrderByDescending(x => x.ReleaseDate)
                .ToListAsync();

            bool changed = false;
            foreach (var form in forms)
            {
                if (!form.IsReleased && form.ReleaseDate <= DateTime.UtcNow)
                {
                    await ReleaseFormAsync(form);
                    changed = true;
                }
                if (!form.IsClosed && form.SubmissionDeadline < DateTime.UtcNow)
                {
                    form.IsClosed = true;
                    changed = true;
                }
            }

            if (changed)
            {
                await _context.SaveChangesAsync();
            }

            return forms.Select(x => new FeedbackFormDto
            {
                Id = x.Id,
                InternshipId = x.InternshipId,
                Title = x.Title,
                Description = x.Description,
                ReleaseDate = x.ReleaseDate,
                SubmissionDeadline = x.SubmissionDeadline,
                IsReleased = x.IsReleased,
                IsClosed = x.IsClosed,
                AllowAnonymous = x.AllowAnonymous,
                MaxRating = x.MaxRating,
                AllowedStatuses = (x.AllowedStatuses ?? "Completed")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToList()
            });
        }
    }
}
