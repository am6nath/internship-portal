using InternshipPortal.API.DTOs.Auth;
using InternshipPortal.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternshipPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // REGISTER
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            var result = await _authService.RegisterAsync(model);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // LOGIN
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var result = await _authService.LoginAsync(model);

            if (!result.IsSuccess)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        // REGISTER ADMIN
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin(
            RegisterAdminDto model)
        {
            var result = await _authService
                .RegisterAdminAsync(model);

            if (!result.IsSuccess)
            {
                return BadRequest(new
                {
                    success = false,
                    statusCode = 400,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = result.Message
            });
        }

        // VERIFY OTP
        [HttpPost("verify-otp")]
        public async Task<IActionResult>
            VerifyOtp(
                VerifyOtpDto model)
        {
            var result = await _authService
                .VerifyOtpAsync(model);

            if (!result.IsSuccess)
            {
                return BadRequest(new
                {
                    success = false,
                    statusCode = 400,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = result.Message
            });
        }

        // FORGOT PASSWORD
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
        {
            var result = await _authService.ForgotPasswordAsync(model);

            if (!result.IsSuccess)
            {
                return BadRequest(new
                {
                    success = false,
                    statusCode = 400,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = result.Message
            });
        }

        // RESET PASSWORD
        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            var result = await _authService.ResetPasswordAsync(model);

            if (!result.IsSuccess)
            {
                return BadRequest(new
                {
                    success = false,
                    statusCode = 400,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = result.Message
            });
        }
    }
}