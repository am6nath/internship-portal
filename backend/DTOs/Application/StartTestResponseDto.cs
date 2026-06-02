namespace InternshipPortal.API.DTOs.Application
{
    public class StartTestResponseDto
    {
        public string TestType { get; set; } = "PreTest";
        public string SessionToken { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
        public int PassingScorePercent { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string InternshipTitle { get; set; } = string.Empty;
        public List<TestQuestionDto> Questions { get; set; } = new();
    }
}
