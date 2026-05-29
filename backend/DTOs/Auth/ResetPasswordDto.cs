namespace InternshipPortal.API.DTOs.Auth
{
    public class ResetPasswordDto
    {
        public string Email { get; set; }
        
        public string OtpCode { get; set; }
        
        public string NewPassword { get; set; }
        
        public string ConfirmNewPassword { get; set; }
    }
}
