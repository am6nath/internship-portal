namespace InternshipPortal.API.DTOs.Application
{
    public class SubmitTestAnswerDto
    {
        public string QuestionId { get; set; } = string.Empty;
        public string SelectedAnswer { get; set; } = string.Empty;
    }

    public class SubmitTestDto
    {
        public string SessionToken { get; set; } = string.Empty;
        public List<SubmitTestAnswerDto> Answers { get; set; } = new();
    }
}
