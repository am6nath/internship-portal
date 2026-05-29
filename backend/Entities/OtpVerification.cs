namespace InternshipPortal.API.Entities
{
    public class OtpVerification : BaseEntity
    {
        // USER
        public Guid UserId { get; set; }

        public ApplicationUser User { get; set; }

        // OTP
        public string OtpCode { get; set; }

        // EXPIRY
        public DateTime ExpiryTime { get; set; }

        // STATUS
        public bool IsVerified { get; set; } = false;

        public bool IsUsed { get; set; } = false;
    }
}