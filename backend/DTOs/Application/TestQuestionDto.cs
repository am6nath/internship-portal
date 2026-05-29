namespace InternshipPortal.API.DTOs.Application
{
    public class TestQuestionDto
    {
        public string Id { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
    }
}
