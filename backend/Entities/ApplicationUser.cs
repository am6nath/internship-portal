using Microsoft.AspNetCore.Identity;

namespace InternshipPortal.API.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FullName { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public StudentProfile? StudentProfile { get; set; }
        public bool IsEmailVerified { get; set; } = false;
    }
}