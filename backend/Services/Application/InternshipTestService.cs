using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using InternshipPortal.API.Data.Context;
using InternshipPortal.API.DTOs.Application;
using InternshipPortal.API.Entities;
using InternshipPortal.API.Enums;
using InternshipPortal.API.Helpers;
using InternshipPortal.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.API.Services.Application
{
    public class InternshipTestService : IInternshipTestService
    {
        private const int QuestionCount = 10;
        private const int PassingScorePercent = 70;
        private const int SessionMinutes = 45;
        private const int PreTestMaxAttempts = 1;
        private const int CompletionTestMaxAttempts = 5;

        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public InternshipTestService(
            ApplicationDbContext context,
            IHttpClientFactory httpClientFactory,
            INotificationService notificationService,
            IEmailService emailService)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public Task<StartTestResponseDto> StartPreTestAsync(Guid applicationId, Guid studentId) =>
            StartTestInternalAsync(applicationId, studentId, TestType.PreTest);

        public Task<SubmitTestResultDto> SubmitPreTestAsync(Guid applicationId, Guid studentId, SubmitTestDto model) =>
            SubmitTestInternalAsync(applicationId, studentId, model, TestType.PreTest);

        public Task<StartTestResponseDto> StartCompletionTestAsync(Guid applicationId, Guid studentId) =>
            StartTestInternalAsync(applicationId, studentId, TestType.CompletionTest);

        public Task<SubmitTestResultDto> SubmitCompletionTestAsync(Guid applicationId, Guid studentId, SubmitTestDto model) =>
            SubmitTestInternalAsync(applicationId, studentId, model, TestType.CompletionTest);

        public async Task<TestStatusResponseDto?> GetTestStatusAsync(Guid applicationId, Guid studentId)
        {
            var application = await _context.Applications
                .Include(x => x.Internship)
                .FirstOrDefaultAsync(x =>
                    x.Id == applicationId &&
                    x.StudentId == studentId &&
                    !x.IsDeleted);

            if (application == null) return null;

            var preTestAttempts = await CountAttemptsAsync(applicationId, studentId, TestType.PreTest);
            var completionAttempts = await CountAttemptsAsync(applicationId, studentId, TestType.CompletionTest);

            return new TestStatusResponseDto
            {
                IsPreTestPassed = application.IsPreTestPassed,
                PreTestScore = application.PreTestScore,
                PreTestPassedAt = application.PreTestPassedAt,
                IsTestPassed = application.IsTestPassed,
                TestScore = application.TestScore,
                TestPassedAt = application.TestPassedAt,
                PassingScorePercent = PassingScorePercent,
                PreTestMaxAttempts = PreTestMaxAttempts,
                PreTestAttemptsUsed = preTestAttempts,
                CanTakePreTest = CanTakePreTest(application, preTestAttempts),
                CompletionTestMaxAttempts = CompletionTestMaxAttempts,
                CompletionTestAttemptsUsed = completionAttempts,
                CanTakeCompletionTest = CanTakeCompletionTest(application, completionAttempts),
                Status = application.Status.ToString()
            };
        }

        private async Task<StartTestResponseDto> StartTestInternalAsync(
            Guid applicationId,
            Guid studentId,
            TestType testType)
        {
            var application = await GetApplicationAsync(applicationId, studentId);
            ValidateCanStart(application, testType);

            if (testType == TestType.PreTest && application.IsPreTestPassed)
            {
                throw new Exception("You have already passed the pre-test for this internship.");
            }

            if (testType == TestType.CompletionTest && application.IsTestPassed)
            {
                throw new Exception("You have already passed the completion test for this internship.");
            }

            var attemptCount = await CountAttemptsAsync(applicationId, studentId, testType);
            var maxAttempts = testType == TestType.PreTest ? PreTestMaxAttempts : CompletionTestMaxAttempts;

            if (attemptCount >= maxAttempts)
            {
                var label = testType == TestType.PreTest ? "pre-test" : "completion test";
                throw new Exception($"Maximum {maxAttempts} {label} attempt(s) reached. Contact your administrator.");
            }

            await InvalidateOpenSessionsAsync(applicationId, studentId, testType);

            var seed = ComputeSeed(applicationId, studentId, attemptCount, testType);
            var storedQuestions = testType == TestType.PreTest
                ? await FetchPreTestQuestionsAsync(seed)
                : await FetchCompletionTestQuestionsAsync(application.Internship, seed);

            var sessionToken = Guid.NewGuid().ToString("N");
            var session = new InternshipTestSession
            {
                ApplicationId = applicationId,
                StudentId = studentId,
                TestType = testType,
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
                TestType = testType.ToString(),
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

        private async Task<SubmitTestResultDto> SubmitTestInternalAsync(
            Guid applicationId,
            Guid studentId,
            SubmitTestDto model,
            TestType testType)
        {
            var application = await GetApplicationAsync(applicationId, studentId);

            var session = await _context.InternshipTestSessions
                .FirstOrDefaultAsync(x =>
                    x.ApplicationId == applicationId &&
                    x.StudentId == studentId &&
                    x.TestType == testType &&
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

            if (testType == TestType.PreTest)
            {
                await HandlePreTestResultAsync(application, passed, scorePercent);
            }
            else
            {
                await HandleCompletionTestResultAsync(application, passed, scorePercent);
            }

            await _context.SaveChangesAsync();

            var testLabel = testType == TestType.PreTest ? "Pre-test" : "Completion test";

            return new SubmitTestResultDto
            {
                Passed = passed,
                ScorePercent = scorePercent,
                CorrectAnswers = correct,
                TotalQuestions = storedQuestions.Count,
                PassingScorePercent = PassingScorePercent,
                Message = passed
                    ? testType == TestType.PreTest
                        ? "Congratulations! You passed the pre-test. Your internship is now in progress."
                        : "Congratulations! You passed the completion test. Admin has been notified to issue your certificate."
                    : testType == TestType.PreTest
                        ? $"You scored {scorePercent}%. Minimum {PassingScorePercent}% required. You cannot retake the pre-test — contact your administrator."
                        : $"You scored {scorePercent}%. Minimum {PassingScorePercent}% required. You may retake the completion test."
            };
        }

        private async Task HandlePreTestResultAsync(Entities.Application application, bool passed, decimal scorePercent)
        {
            if (passed)
            {
                application.IsPreTestPassed = true;
                application.PreTestScore = scorePercent;
                application.PreTestPassedAt = DateTime.UtcNow;
                application.Status = ApplicationStatus.InProgress;
                application.UpdatedAt = DateTime.UtcNow;

                await _notificationService.CreateNotificationAsync(
                    application.StudentId,
                    "Pre-test Passed",
                    $"You passed the pre-test ({scorePercent}%) for '{application.Internship.Title}'. Your internship is now in progress.",
                    "TestResult"
                );

                await _notificationService.CreateNotificationAsync(
                    application.Internship.AdminId,
                    "Student Passed Pre-test",
                    $"{application.Student.FullName} passed the pre-test ({scorePercent}%) for '{application.Internship.Title}'.",
                    "TestPassed"
                );

                await _emailService.SendEmailAsync(
                    application.Student.Email!,
                    $"Pre-test Passed - {application.Internship.Title}",
                    BuildTestResultEmail(application.Student.FullName, application.Internship.Title, "pre-test", scorePercent, true)
                );

                await _emailService.SendEmailAsync(
                    application.Internship.Admin!.Email!,
                    $"Student Passed Pre-test - {application.Internship.Title}",
                    $@"Hello,

{application.Student.FullName} has passed the pre-test for '{application.Internship.Title}' with a score of {scorePercent}%.

The student's internship status is now In Progress.

Internship Portal"
                );
            }
            else
            {
                await _notificationService.CreateNotificationAsync(
                    application.StudentId,
                    "Pre-test Failed",
                    $"You scored {scorePercent}% on the pre-test for '{application.Internship.Title}'. Minimum {PassingScorePercent}% required. Contact your administrator.",
                    "TestResult"
                );

                await _emailService.SendEmailAsync(
                    application.Student.Email!,
                    $"Pre-test Result - {application.Internship.Title}",
                    BuildTestResultEmail(application.Student.FullName, application.Internship.Title, "pre-test", scorePercent, false)
                );
            }
        }

        private async Task HandleCompletionTestResultAsync(Entities.Application application, bool passed, decimal scorePercent)
        {
            if (passed)
            {
                application.IsTestPassed = true;
                application.TestScore = scorePercent;
                application.TestPassedAt = DateTime.UtcNow;
                application.UpdatedAt = DateTime.UtcNow;

                await _notificationService.CreateNotificationAsync(
                    application.StudentId,
                    "Completion Test Passed",
                    $"You passed the completion test ({scorePercent}%) for '{application.Internship.Title}'. Your certificate will be issued soon.",
                    "TestResult"
                );

                await _notificationService.CreateNotificationAsync(
                    application.Internship.AdminId,
                    "Certificate Ready to Issue",
                    $"{application.Student.FullName} passed the completion test ({scorePercent}%) for '{application.Internship.Title}'. Please verify and issue the certificate.",
                    "TestPassed"
                );

                await _emailService.SendEmailAsync(
                    application.Student.Email!,
                    $"Completion Test Passed - {application.Internship.Title}",
                    BuildTestResultEmail(application.Student.FullName, application.Internship.Title, "completion test", scorePercent, true)
                );

                await _emailService.SendEmailAsync(
                    application.Internship.Admin!.Email!,
                    $"Student Passed Completion Test - {application.Internship.Title}",
                    $@"Hello,

{application.Student.FullName} has passed the completion test for '{application.Internship.Title}' with a score of {scorePercent}%.

Please verify and issue the certificate from the admin portal.

Internship Portal"
                );
            }
            else
            {
                await _notificationService.CreateNotificationAsync(
                    application.StudentId,
                    "Completion Test Failed",
                    $"You scored {scorePercent}% on the completion test for '{application.Internship.Title}'. Minimum {PassingScorePercent}% required. You may retake the test.",
                    "TestResult"
                );

                await _emailService.SendEmailAsync(
                    application.Student.Email!,
                    $"Completion Test Result - {application.Internship.Title}",
                    BuildTestResultEmail(application.Student.FullName, application.Internship.Title, "completion test", scorePercent, false)
                );
            }
        }

        private static string BuildTestResultEmail(
            string studentName,
            string internshipTitle,
            string testLabel,
            decimal scorePercent,
            bool passed) =>
            $@"Hello {studentName},

Your {testLabel} result for '{internshipTitle}' is available.

Score: {scorePercent}%
Minimum required: {PassingScorePercent}%
Result: {(passed ? "PASSED" : "FAILED")}

{(passed ? "Great work! Check your portal for next steps." : "Review the material and try again if attempts remain.")}

Thank you,
Internship Portal";

        private async Task<Entities.Application> GetApplicationAsync(Guid applicationId, Guid studentId)
        {
            var application = await _context.Applications
                .Include(x => x.Internship)
                    .ThenInclude(i => i.Admin)
                .Include(x => x.Student)
                .FirstOrDefaultAsync(x =>
                    x.Id == applicationId &&
                    x.StudentId == studentId &&
                    !x.IsDeleted);

            if (application == null)
            {
                throw new Exception("Application not found.");
            }

            return application;
        }

        private static void ValidateCanStart(Entities.Application application, TestType testType)
        {
            if (application.IsCompleted)
            {
                throw new Exception("This internship application is already completed.");
            }

            if (testType == TestType.PreTest)
            {
                if (application.Status != ApplicationStatus.Accepted)
                {
                    throw new Exception("Pre-test is only available after your application has been accepted.");
                }
            }
            else if (!application.IsPreTestPassed)
            {
                throw new Exception("You must pass the pre-test before taking the completion test.");
            }
            else if (application.Status != ApplicationStatus.InProgress)
            {
                throw new Exception("Completion test is only available for in-progress internships.");
            }
        }

        private static bool CanTakePreTest(Entities.Application application, int attemptsUsed)
        {
            if (application.IsCompleted || application.IsPreTestPassed) return false;
            if (application.Status != ApplicationStatus.Accepted) return false;
            return attemptsUsed < PreTestMaxAttempts;
        }

        private static bool CanTakeCompletionTest(Entities.Application application, int attemptsUsed)
        {
            if (application.IsCompleted || application.IsTestPassed) return false;
            if (!application.IsPreTestPassed) return false;
            if (application.Status != ApplicationStatus.InProgress) return false;
            return attemptsUsed < CompletionTestMaxAttempts;
        }

        private Task<int> CountAttemptsAsync(Guid applicationId, Guid studentId, TestType testType) =>
            _context.InternshipTestSessions.CountAsync(x =>
                x.ApplicationId == applicationId &&
                x.StudentId == studentId &&
                x.TestType == testType &&
                !x.IsDeleted);

        private async Task InvalidateOpenSessionsAsync(Guid applicationId, Guid studentId, TestType testType)
        {
            var openSessions = await _context.InternshipTestSessions
                .Where(x =>
                    x.ApplicationId == applicationId &&
                    x.StudentId == studentId &&
                    x.TestType == testType &&
                    !x.IsSubmitted &&
                    !x.IsDeleted &&
                    x.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

            foreach (var s in openSessions)
            {
                s.IsDeleted = true;
            }
        }

        private async Task<List<StoredQuestion>> FetchPreTestQuestionsAsync(int seed)
        {
            // Mix local CS questions (algorithms, complexity) with OpenTDB computer science category
            var csQuestions = CsQuestionBank.GetRandomQuestions(6, seed);
            var apiQuestions = await FetchOpenTdbQuestionsAsync(18, seed + 1, 4);

            var combined = new List<StoredQuestion>();
            var index = 0;

            foreach (var (id, question, options, correct) in csQuestions)
            {
                combined.Add(new StoredQuestion
                {
                    Id = $"q{++index}",
                    Question = question,
                    Options = options,
                    CorrectAnswer = correct
                });
            }

            foreach (var q in apiQuestions)
            {
                q.Id = $"q{++index}";
                combined.Add(q);
            }

            var rng = new Random(seed);
            return combined.OrderBy(_ => rng.Next()).Take(QuestionCount).ToList();
        }

        private async Task<List<StoredQuestion>> FetchCompletionTestQuestionsAsync(
            Entities.Internship internship,
            int seed)
        {
            var categoryId = MapInternshipToCategory(internship);
            return await FetchOpenTdbQuestionsAsync(categoryId, seed, QuestionCount);
        }

        private async Task<List<StoredQuestion>> FetchOpenTdbQuestionsAsync(int categoryId, int seed, int count)
        {
            var client = _httpClientFactory.CreateClient("OpenTdb");
            var url =
                $"https://opentdb.com/api.php?amount={count * 2}&category={categoryId}&type=multiple&encode=base64";

            var response = await client.GetFromJsonAsync<OpenTdbResponse>(url);

            if (response?.Results == null || response.Results.Count < count)
            {
                url = $"https://opentdb.com/api.php?amount={count * 2}&type=multiple&encode=base64";
                response = await client.GetFromJsonAsync<OpenTdbResponse>(url);
            }

            if (response?.Results == null || response.Results.Count == 0)
            {
                // Fallback to CS bank for completion test too
                return CsQuestionBank.GetRandomQuestions(count, seed)
                    .Select((q, i) => new StoredQuestion
                    {
                        Id = $"q{i + 1}",
                        Question = q.Question,
                        Options = q.Options,
                        CorrectAnswer = q.CorrectAnswer
                    }).ToList();
            }

            var rng = new Random(seed);
            var shuffled = response.Results.OrderBy(_ => rng.Next()).Take(count).ToList();
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
                return 18;
            }

            if (text.Contains("science") || text.Contains("electronics") || text.Contains("mechanical"))
            {
                return 17;
            }

            if (text.Contains("business") || text.Contains("marketing") || text.Contains("finance"))
            {
                return 9;
            }

            if (text.Contains("history")) return 23;
            if (text.Contains("geography") || text.Contains("civil")) return 22;

            return 18;
        }

        private static int ComputeSeed(Guid applicationId, Guid studentId, int attemptOffset, TestType testType)
        {
            var bytes = applicationId.ToByteArray()
                .Concat(studentId.ToByteArray())
                .Concat(BitConverter.GetBytes(attemptOffset))
                .Concat(BitConverter.GetBytes((int)testType))
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
