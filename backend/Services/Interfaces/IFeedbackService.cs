using InternshipPortal.API.DTOs.Feedback;

namespace InternshipPortal.API.Services.Interfaces
{
    public interface IFeedbackService
    {
        // ADMIN
        Task CreateFeedbackFormAsync(
            Guid adminId,
            CreateFeedbackFormDto model);

        // STUDENT
        Task SubmitFeedbackAsync(
            Guid studentId,
            SubmitFeedbackDto model);

        // GET INTERNSHIP FEEDBACKS
        Task<IEnumerable<FeedbackResponseDto>>
            GetInternshipFeedbacksAsync(
                Guid internshipId);

        // GET STUDENT FEEDBACKS
        Task<IEnumerable<FeedbackResponseDto>>
            GetStudentFeedbacksAsync(
                Guid studentId);

        // GET FEEDBACK FORMS FOR INTERNSHIP
        Task<IEnumerable<FeedbackFormDto>>
            GetFeedbackFormsAsync(
                Guid internshipId);
    }
}


