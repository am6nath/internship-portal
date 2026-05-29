using InternshipPortal.API.Entities;

namespace InternshipPortal.API.Services.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateToken(ApplicationUser user);
    }
}