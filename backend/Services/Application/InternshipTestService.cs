using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using InternshipPortal.API.Data.Context;
using InternshipPortal.API.DTOs.Application;
using InternshipPortal.API.Entities;
using InternshipPortal.API.Enums;
using InternshipPortal.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.API.Services.Application
{
    public class InternshipTestService : IInternshipTestService
    {
        private const int QuestionCount = 10;
        private const int PassingScorePercent = 70;
        private const int SessionMinutes = 45;
        private const int MaxAttemptsPerApplication = 5;

        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly INotificationService _notificationService;

        public InternshipTestService(
            ApplicationDbContext context,
            IHttpClientFactory httpClientFactory,
            INotificationService notificationService)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _notificationService = notificationService;
        }

        public async Task<StartTestResponseDto> StartTestAsync(
            Guid applicationId,
            Guid studentId)
        {
            var application = await GetEligibleApplicationAsync(applicationId, studentId);

            if (application.IsTestPassed)
            {
                throw new Exception("You have already passed the completion test for this internship.");
            }

            var attemptCount = await _context.InternshipTestSessions
                .CountAsync(x =>
                    x.ApplicationId == applicationId &&
                    x.StudentId == studentId &&
                    !x.IsDeleted);

            if (attemptCount >= MaxAttemptsPerApplication)
            {
                throw new Exception($"Maximum {MaxAttemptsPerApplication} test attempts reached. Contact your administrator.");
            }

            // Invalidate any open sessions
            var openSessions = await _context.InternshipTestSessions
                .Where(x =>
                    x.ApplicationId == applicationId &&
                    x.StudentId == studentId &&
                    !x.IsSubmitted &&
                    !x.IsDeleted &&
                    x.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

            foreach (var s in openSessions)
            {
                s.IsDeleted = true;
            }

            var categoryId = MapInternshipToCategory(application.Internship);
            var seed = ComputeSeed(applicationId, studentId, attemptCount);
            var storedQuestions = await FetchUniqueQuestionsAsync(categoryId, seed);

            var sessionToken = Guid.NewGuid().ToString("N");
            var session = new InternshipTestSession
            {
                ApplicationId = applicationId,
                StudentId = studentId,
                SessionToken = sessionToken,
                QuestionsJson = JsonSerializer.Serialize(storedQuestions),
                TotalQuestions = storedQuestions.Count,
                ExpiresAt = DateTime.UtcNow.AddMinutes(SessionMinutes),
                CreatedBy = studentId
            };

            _context.InternshipTestSessions.Add(session);
            await _context.SaveChangesAsync();

            return new StartTestResponseDto
            {
                SessionToken = sessionToken,
                TotalQuestions = storedQuestions.Count,
                PassingScorePercent = PassingScorePercent,
                ExpiresAt = session.ExpiresAt,
                InternshipTitle = application.Internship.Title,
                Questions = storedQuestions.Select(q => new TestQuestionDto
                {
                    Id = q.Id,
                    Question = q.Question,
                    Options = q.Options
                }).ToList()
            };
        }

        public async Task<SubmitTestResultDto> SubmitTestAsync(
            Guid applicationId,
            Guid studentId,
            SubmitTestDto model)
        {
            var application = await GetEligibleApplicationAsync(applicationId, studentId);

            var session = await _context.InternshipTestSessions
                .FirstOrDefaultAsync(x =>
                    x.ApplicationId == applicationId &&
                    x.StudentId == studentId &&
                    x.SessionToken == model.SessionToken &&
                    !x.IsDeleted);

            if (session == null)
            {
                throw new Exception("Test session not found. Please start a new test.");
            }

            if (session.IsSubmitted)
            {
                throw new Exception("This test session was already submitted.");
            }

            if (session.ExpiresAt < DateTime.UtcNow)
            {
                throw new Exception("Test session expired. Please start a new test.");
            }

            var storedQuestions = JsonSerializer.Deserialize<List<StoredQuestion>>(session.QuestionsJson)
                ?? throw new Exception("Invalid test session data.");

            var answerMap = model.Answers
                .GroupBy(a => a.QuestionId)
                .ToDictionary(g => g.Key, g => g.Last().SelectedAnswer.Trim());

            int correct = 0;
            foreach (var q in storedQuestions)
            {
                if (answerMap.TryGetValue(q.Id, out var selected) &&
                    string.Equals(NormalizeAnswer(selected), NormalizeAnswer(q.CorrectAnswer), StringComparison.OrdinalIgnoreCase))
                {
                    correct++;
                }
            }

            var scorePercent = Math.Round((decimal)correct / storedQuestions.Count * 100, 2);
            var passed = scorePercent >= PassingScorePercent;

            session.IsSubmitted = true;
            session.CorrectAnswers = correct;
            session.ScorePercent = scorePercent;
            session.IsPassed = passed;
            session.SubmittedAt = DateTime.UtcNow;
            session.UpdatedAt = DateTime.UtcNow;

            if (passed)
            {
                application.IsTestPassed = true;
                application.TestScore = scorePercent;
                application.TestPassedAt = DateTime.UtcNow;

                if (application.Status == ApplicationStatus.Accepted)
                {
                    application.Status = ApplicationStatus.InProgress;
                }

                application.UpdatedAt = DateTime.UtcNow;

                var adminId = application.Internship.AdminId;
                await _notificationService.CreateNotificationAsync(
                    adminId,
                    "Certificate Ready to Issue",
                    $"{application.Student.FullName} passed the completion test ({scorePercent}%) for '{application.Internship.Title}'. Please verify and issue the certificate.",
                    "TestPassed"
                );
            }

            await _context.SaveChangesAsync();

            return new SubmitTestResultDto
            {
                Passed = passed,
                ScorePercent = scorePercent,
                CorrectAnswers = correct,
                TotalQuestions = storedQuestions.Count,
                PassingScorePercent = PassingScorePercent,
                Message = passed
                    ? "Congratulations! You passed the test. Admin has been notified to issue your certificate."
                    : $"You scored {scorePercent}%. Minimum {PassingScorePercent}% required. You may retake the test."
            };
        }

        public async Task<object?> GetTestStatusAsync(Guid applicationId, Guid studentId)
        {
            var application = await _context.Applications
                .Include(x => x.Internship)
                .FirstOrDefaultAsync(x =>
                    x.Id == applicationId &&
                    x.StudentId == studentId &&
                    !x.IsDeleted);

            if (application == null) return null;

            var attempts = await _context.InternshipTestSessions
                .CountAsync(x =>
                    x.ApplicationId == applicationId &&
                    x.StudentId == studentId &&
                    !x.IsDeleted);

            return new
            {
                application.IsTestPassed,
                application.TestScore,
                application.TestPassedAt,
                PassingScorePercent,
                MaxAttempts = MaxAttemptsPerApplication,
                AttemptsUsed = attempts,
                CanTakeTest = CanTakeTest(application),
                application.Status
            };
        }

        private async Task<Entities.Application> GetEligibleApplicationAsync(
            Guid applicationId,
            Guid studentId)
        {
            var application = await _context.Applications
                .Include(x => x.Internship)
                .Include(x => x.Student)
                .FirstOrDefaultAsync(x =>
                    x.Id == applicationId &&
                    x.StudentId == studentId &&
                    !x.IsDeleted);

            if (application == null)
            {
                throw new Exception("Application not found.");
            }

            if (!CanTakeTest(application))
            {
                throw new Exception(
                    "Completion test is only available for accepted or in-progress internships that are not yet completed.");
            }

            return application;
        }

        private static bool CanTakeTest(Entities.Application application)
        {
            if (application.IsCompleted) return false;

            return application.Status is ApplicationStatus.Accepted
                or ApplicationStatus.InProgress;
        }

        private async Task<List<StoredQuestion>> FetchUniqueQuestionsAsync(int categoryId, int seed)
        {
            var client = _httpClientFactory.CreateClient("OpenTdb");
            var url =
                $"https://opentdb.com/api.php?amount={QuestionCount * 2}&category={categoryId}&type=multiple&encode=base64";

            var response = await client.GetFromJsonAsync<OpenTdbResponse>(url);

            if (response?.Results == null || response.Results.Count < QuestionCount)
            {
                // Fallback without category
                url = $"https://opentdb.com/api.php?amount={QuestionCount * 2}&type=multiple&encode=base64";
                response = await client.GetFromJsonAsync<OpenTdbResponse>(url);
            }

            if (response?.Results == null || response.Results.Count == 0)
            {
                throw new Exception("Unable to load test questions. Please try again later.");
            }

            var rng = new Random(seed);
            var shuffled = response.Results.OrderBy(_ => rng.Next()).Take(QuestionCount).ToList();

            var questions = new List<StoredQuestion>();
            for (var i = 0; i < shuffled.Count; i++)
            {
                var item = shuffled[i];
                var questionText = DecodeBase64(item.Question);
                var correct = DecodeBase64(item.CorrectAnswer);
                var incorrect = item.IncorrectAnswers.Select(DecodeBase64).ToList();

                var options = new List<string> { correct };
                options.AddRange(incorrect);
                options = options.OrderBy(_ => rng.Next()).ToList();

                questions.Add(new StoredQuestion
                {
                    Id = $"q{i + 1}",
                    Question = questionText,
                    Options = options,
                    CorrectAnswer = correct
                });
            }

            return questions;
        }

        private static int MapInternshipToCategory(Entities.Internship internship)
        {
            var text = $"{internship.RequiredSkills} {internship.AllowedDepartments} {internship.Title}".ToLowerInvariant();

            if (text.Contains("angular") || text.Contains("react") || text.Contains("java")
                || text.Contains("python") || text.Contains("c#") || text.Contains("computer")
                || text.Contains("software") || text.Contains("developer") || text.Contains("it"))
            {
                return 18; // Science: Computers
            }

            if (text.Contains("science") || text.Contains("electronics") || text.Contains("mechanical"))
            {
                return 17; // Science & Nature
            }

            if (text.Contains("business") || text.Contains("marketing") || text.Contains("finance"))
            {
                return 9; // General Knowledge
            }

            if (text.Contains("history")) return 23;
            if (text.Contains("geography") || text.Contains("civil")) return 22;

            return 9;
        }

        private static int ComputeSeed(Guid applicationId, Guid studentId, int attemptOffset)
        {
            var bytes = applicationId.ToByteArray()
                .Concat(studentId.ToByteArray())
                .Concat(BitConverter.GetBytes(attemptOffset))
                .ToArray();
            return BitConverter.ToInt32(SHA256.HashData(bytes), 0);
        }

        private static string DecodeBase64(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(value));
            }
            catch
            {
                return value;
            }
        }

        private static string NormalizeAnswer(string value) =>
            value.Trim().Replace("&quot;", "\"").Replace("&#039;", "'");

        private class StoredQuestion
        {
            public string Id { get; set; } = string.Empty;
            public string Question { get; set; } = string.Empty;
            public List<string> Options { get; set; } = new();
            public string CorrectAnswer { get; set; } = string.Empty;
        }

        private class OpenTdbResponse
        {
            [JsonPropertyName("response_code")]
            public int ResponseCode { get; set; }

            [JsonPropertyName("results")]
            public List<OpenTdbQuestion> Results { get; set; } = new();
        }

        private class OpenTdbQuestion
        {
            [JsonPropertyName("question")]
            public string Question { get; set; } = string.Empty;

            [JsonPropertyName("correct_answer")]
            public string CorrectAnswer { get; set; } = string.Empty;

            [JsonPropertyName("incorrect_answers")]
            public List<string> IncorrectAnswers { get; set; } = new();
        }
    }
}
