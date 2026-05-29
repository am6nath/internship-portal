using InternshipPortal.API.Data.Context;
using InternshipPortal.API.DTOs.Auth;
using InternshipPortal.API.Entities;
using InternshipPortal.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.API.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IJwtService _jwtService;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IJwtService jwtService,
            ApplicationDbContext context,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
            _context = context;
            _emailService = emailService;
        }

        // REGISTER USER
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto model)
        {
            // CHECK EXISTING USER
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Email already exists" };
            }

            // PASSWORD MATCH CHECK
            if (model.Password != model.ConfirmPassword)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Passwords do not match" };
            }

            // CREATE USER
            var user = new ApplicationUser
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email,
                IsEmailVerified = false
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            // ASSIGN ROLE
            await _userManager.AddToRoleAsync(user, model.Role);

            // GENERATE OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // STORE OTP
            var otpVerification = new OtpVerification
            {
                UserId = user.Id,
                OtpCode = otp,
                ExpiryTime = DateTime.UtcNow.AddMinutes(10),
                CreatedBy = user.Id
            };

            _context.OtpVerifications.Add(otpVerification);
            await _context.SaveChangesAsync();

            // EMAIL BODY
            var emailBody = $"Your OTP code is: {otp}\n\nOTP expires in 10 minutes.";

            Console.WriteLine($"Generated OTP: {otp}");
            Console.WriteLine($"Sending OTP to: {user.Email}");

            try
            {
                await _emailService.SendEmailAsync(user.Email!, $"Internship Portal OTP - {otp}", emailBody);
                Console.WriteLine("OTP email sent successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OTP email sending failed: {ex.Message}");
            }

            return new AuthResponseDto { IsSuccess = true, Message = "Registration successful. OTP sent successfully." };
        }

        // LOGIN
        public async Task<AuthResponseDto> LoginAsync(LoginDto model)
        {
            // FIND USER
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid email or password" };
            }

            // EMAIL VERIFICATION CHECK
            if (!user.IsEmailVerified)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Please verify your email before login" };
            }

            // PASSWORD CHECK
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, true);
            if (!result.Succeeded)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid email or password" };
            }

            // GENERATE JWT
            var token = await _jwtService.GenerateToken(user);

            // GET ROLE
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Login successful",
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(60),
                Role = roles.FirstOrDefault()
            };
        }

        // REGISTER ADMIN
        public async Task<AuthResponseDto> RegisterAdminAsync(RegisterAdminDto model)
        {
            // CHECK EMAIL
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Email already exists" };
            }

            // CREATE ADMIN USER
            var user = new ApplicationUser
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email,
                IsEmailVerified = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Admin registration failed" };
            }

            // ASSIGN ADMIN ROLE
            await _userManager.AddToRoleAsync(user, "Admin");

            return new AuthResponseDto { IsSuccess = true, Message = "Admin registered successfully" };
        }

        // VERIFY OTP
        public async Task<AuthResponseDto> VerifyOtpAsync(VerifyOtpDto model)
        {
            // FIND USER
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "User not found" };
            }

            // FIND OTP
            var otpVerification = await _context.OtpVerifications
                .Where(x => x.UserId == user.Id && !x.IsUsed && !x.IsVerified)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpVerification == null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "OTP not found" };
            }

            // CHECK OTP MATCH
            if (otpVerification.OtpCode != model.OtpCode)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid OTP" };
            }

            // CHECK OTP EXPIRY
            if (otpVerification.ExpiryTime < DateTime.UtcNow)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "OTP expired" };
            }

            // MARK OTP VERIFIED
            otpVerification.IsVerified = true;
            otpVerification.IsUsed = true;
            otpVerification.UpdatedAt = DateTime.UtcNow;

            // VERIFY USER EMAIL
            user.IsEmailVerified = true;
            await _context.SaveChangesAsync();

            return new AuthResponseDto { IsSuccess = true, Message = "Email verified successfully" };
        }

        // FORGOT PASSWORD
        public async Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto model)
        {
            // FIND USER
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "User not found" };
            }

            // GENERATE OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // STORE OTP
            var otpVerification = new OtpVerification
            {
                UserId = user.Id,
                OtpCode = otp,
                ExpiryTime = DateTime.UtcNow.AddMinutes(10),
                CreatedBy = user.Id
            };

            _context.OtpVerifications.Add(otpVerification);
            await _context.SaveChangesAsync();

            // EMAIL BODY
            var emailBody = $"Your OTP code for password reset is: {otp}\n\nOTP expires in 10 minutes.";

            Console.WriteLine($"Generated Forgot Password OTP: {otp}");
            Console.WriteLine($"Sending Forgot Password OTP to: {user.Email}");

            try
            {
                await _emailService.SendEmailAsync(user.Email!, $"Internship Portal Password Reset OTP - {otp}", emailBody);
                Console.WriteLine("Forgot Password OTP email sent successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Forgot Password OTP email sending failed: {ex.Message}");
            }

            return new AuthResponseDto { IsSuccess = true, Message = "OTP sent successfully." };
        }

        // RESET PASSWORD
        public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto model)
        {
            // FIND USER
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "User not found" };
            }

            // PASSWORD MATCH CHECK
            if (model.NewPassword != model.ConfirmNewPassword)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Passwords do not match" };
            }

            // FIND OTP
            var otpVerification = await _context.OtpVerifications
                .Where(x => x.UserId == user.Id && !x.IsUsed && !x.IsVerified)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpVerification == null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "OTP not found" };
            }

            // CHECK OTP MATCH
            if (otpVerification.OtpCode != model.OtpCode)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid OTP" };
            }

            // CHECK OTP EXPIRY
            if (otpVerification.ExpiryTime < DateTime.UtcNow)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "OTP expired" };
            }

            // MARK OTP VERIFIED
            otpVerification.IsVerified = true;
            otpVerification.IsUsed = true;
            otpVerification.UpdatedAt = DateTime.UtcNow;

            // VERIFY USER EMAIL (just in case they forgot password before verifying email)
            user.IsEmailVerified = true;

            // RESET PASSWORD
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            await _context.SaveChangesAsync();
            return new AuthResponseDto { IsSuccess = true, Message = "Password reset successful" };
        }
    }
}