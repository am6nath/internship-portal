using InternshipPortal.API.DTOs.Auth;

namespace InternshipPortal.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto model);

        Task<AuthResponseDto> LoginAsync(LoginDto model);

        Task<AuthResponseDto> RegisterAdminAsync(
    RegisterAdminDto model);

        Task<AuthResponseDto>
        VerifyOtpAsync(
            VerifyOtpDto model);

        Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto model);

        Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto model);
    }
}