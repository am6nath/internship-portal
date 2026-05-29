using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using InternshipPortal.API.Data.Context;
using InternshipPortal.API.Data.Seeders;
using InternshipPortal.API.Entities;
using InternshipPortal.API.Services.Auth;
using InternshipPortal.API.Services.File;
using InternshipPortal.API.Services.Interfaces;
using InternshipPortal.API.Services.Student;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using InternshipPortal.API.Services.Internship;
using InternshipPortal.API.Services.Application;
using InternshipPortal.API.Services.Email;
using InternshipPortal.API.Services.Feedback;
using InternshipPortal.API.Services.TrainingMaterial;
using InternshipPortal.API.Services.Notification;
using InternshipPortal.API.Middleware;


var builder = WebApplication.CreateBuilder(args);

// ADD CONTROLLERS
builder.Services.AddControllers();

// MYSQL CONNECTION
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )
    )
);

// ASP.NET IDENTITY
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    // PASSWORD SETTINGS
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;

    // USER SETTINGS
    options.User.RequireUniqueEmail = true;

    // LOCKOUT SETTINGS
    options.Lockout.DefaultLockoutTimeSpan =
        TimeSpan.FromMinutes(5);

    options.Lockout.MaxFailedAccessAttempts = 5;

    options.Lockout.AllowedForNewUsers = true;

})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT AUTHENTICATION
var jwtKey = builder.Configuration["Jwt:Key"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;

})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;

    options.SaveToken = true;

    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer =
                builder.Configuration["Jwt:Issuer"],

            ValidAudience =
                builder.Configuration["Jwt:Audience"],

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey!)
                )
        };
});

// AUTHORIZATION
builder.Services.AddAuthorization();

// FLUENT VALIDATION
builder.Services
    .AddFluentValidationAutoValidation();

builder.Services
    .AddValidatorsFromAssemblyContaining<Program>();

// SWAGGER
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// DEPENDENCY INJECTION
builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<
    IStudentProfileService,
    StudentProfileService>();

builder.Services.AddScoped<
    IFileService,
    FileService>();

builder.Services.AddScoped<
    IInternshipService,
    InternshipService>();

builder.Services.AddScoped<
    IEligibilityService,
    EligibilityService>();

builder.Services.AddScoped<
    IApplicationService,
    ApplicationService>();

builder.Services.AddScoped<
    IEmailService,
    EmailService>();
builder.Services.AddHttpClient("OpenTdb", client =>
{
    client.BaseAddress = new Uri("https://opentdb.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<IInternshipTestService, InternshipTestService>();

builder.Services.AddScoped<
    IFeedbackService,
    FeedbackService>();
builder.Services.AddScoped<
    ITrainingMaterialService,
    TrainingMaterialService>();
builder.Services.AddScoped<
    INotificationService,
    NotificationService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});



var app = builder.Build();

// SWAGGER
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

// HTTPS
app.UseHttpsRedirection();

// CORS
app.UseCors("AllowAngular");

// STATIC FILES
app.UseStaticFiles();

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("AllowAngular");
// AUTHENTICATION
app.UseAuthentication();

// AUTHORIZATION
app.UseAuthorization();

// MAP CONTROLLERS
app.MapControllers();

// ROLE SEEDING
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    await RoleSeeder.SeedRolesAsync(roleManager);
}

app.Run();