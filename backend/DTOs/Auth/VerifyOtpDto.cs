namespace InternshipPortal.API.DTOs.Auth
{
    public class VerifyOtpDto
    {
        public string Email { get; set; }

        public string OtpCode { get; set; }
    }
}