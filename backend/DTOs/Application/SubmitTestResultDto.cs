namespace InternshipPortal.API.DTOs.Application
{
    public class SubmitTestResultDto
    {
        public bool Passed { get; set; }
        public decimal ScorePercent { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public int PassingScorePercent { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
