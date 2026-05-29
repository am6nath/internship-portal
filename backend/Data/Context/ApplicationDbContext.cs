using InternshipPortal.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InternshipPortal.API.Enums;

namespace InternshipPortal.API.Data.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser,
                                                           IdentityRole<Guid>,
                                                           Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<StudentProfile> StudentProfiles { get; set; }
        public DbSet<OtpVerification> OtpVerifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<Internship> Internships { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<FeedbackForm> FeedbackForms { get; set; }

        public DbSet<FeedbackSubmission> FeedbackSubmissions { get; set; }
        public DbSet<TrainingMaterial> TrainingMaterials { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<InternshipTestSession> InternshipTestSessions { get; set; }

        public override async Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        break;

                    case EntityState.Modified:

                        entry.Property(x => x.CreatedAt).IsModified = false;

                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Refresh Token Relationship
            builder.Entity<RefreshToken>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // OTP Relationship
            builder.Entity<OtpVerification>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentProfile>()
                .HasOne(s => s.User)
                .WithOne(u => u.StudentProfile)
                .HasForeignKey<StudentProfile>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Internship>()
                .HasOne(i => i.Admin)
                .WithMany()
                .HasForeignKey(i => i.AdminId)
                .OnDelete(DeleteBehavior.Restrict);
            // APPLICATION -> STUDENT
            builder.Entity<Application>()
                .HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // APPLICATION -> INTERNSHIP
            builder.Entity<Application>()
                .HasOne(a => a.Internship)
                .WithMany()
                .HasForeignKey(a => a.InternshipId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Entities.Application>()
                .Property(x => x.Status)
                .HasConversion<string>();


            // FEEDBACK FORM -> INTERNSHIP
            builder.Entity<FeedbackForm>()
                .HasOne(x => x.Internship)
                .WithMany(x => x.FeedbackForms)
                .HasForeignKey(x => x.InternshipId)
                .OnDelete(DeleteBehavior.Cascade);

            // FEEDBACK SUBMISSION -> FEEDBACK FORM
            builder.Entity<FeedbackSubmission>()
                .HasOne(x => x.FeedbackForm)
                .WithMany(x => x.FeedbackSubmissions)
                .HasForeignKey(x => x.FeedbackFormId)
                .OnDelete(DeleteBehavior.Cascade);

            // FEEDBACK SUBMISSION -> STUDENT
            builder.Entity<FeedbackSubmission>()
                .HasOne(x => x.Student)
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // FEEDBACK SUBMISSION -> APPLICATION
            builder.Entity<FeedbackSubmission>()
                .HasOne(x => x.Application)
                .WithMany()
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            // TRAINING MATERIAL -> INTERNSHIP
            builder.Entity<TrainingMaterial>()
                .HasOne(x => x.Internship)
                .WithMany(x => x.TrainingMaterials)
                .HasForeignKey(x => x.InternshipId)
                .OnDelete(DeleteBehavior.Cascade);

            // NOTIFICATION -> USER
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InternshipTestSession>()
                .HasOne(x => x.Application)
                .WithMany()
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}