namespace InternshipPortal.API.DTOs.Auth
{
    public class AuthResponseDto
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public string Token { get; set; }

        public DateTime Expiration { get; set; }

        public string Role { get; set; }
    }
}   