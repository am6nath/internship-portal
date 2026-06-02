using InternshipPortal.API.Entities;

namespace InternshipPortal.API.Helpers
{
    public static class EligibilityHelper
    {
        public static bool DepartmentMatches(string allowedDepartments, string studentDepartment)
        {
            if (string.IsNullOrWhiteSpace(allowedDepartments) ||
                string.IsNullOrWhiteSpace(studentDepartment))
            {
                return false;
            }

            var student = studentDepartment.Trim();
            var allowed = allowedDepartments
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(d => d.Trim())
                .Where(d => d.Length > 0)
                .ToList();

            if (allowed.Count == 0)
            {
                return allowedDepartments.Contains(student, StringComparison.OrdinalIgnoreCase);
            }

            return allowed.Any(d =>
                string.Equals(d, student, StringComparison.OrdinalIgnoreCase) ||
                d.Contains(student, StringComparison.OrdinalIgnoreCase) ||
                student.Contains(d, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsOpenForRegistration(Internship internship, DateTime? now = null)
        {
            var utcNow = now ?? DateTime.UtcNow;

            return !internship.IsDeleted &&
                   !internship.IsExpired &&
                   !internship.IsSeatsFilled &&
                   internship.AvailableSeats > 0 &&
                   internship.RegistrationDeadline.Date >= utcNow.Date;
        }

        public static (bool IsEligible, string? Reason) CheckStudentEligibility(
            StudentProfile profile,
            Internship internship)
        {
            if (profile.CGPA < internship.MinimumCGPA)
            {
                return (false, $"Minimum CGPA required: {internship.MinimumCGPA}");
            }

            if (profile.Backlogs > internship.AllowedBacklogs)
            {
                return (false, $"Maximum allowed backlogs: {internship.AllowedBacklogs}");
            }

            if (!DepartmentMatches(internship.AllowedDepartments, profile.Department))
            {
                return (false, $"Department '{profile.Department}' is not in allowed list: {internship.AllowedDepartments}");
            }

            if (profile.GraduationYear != internship.GraduationYear)
            {
                return (false, $"Graduation year must be {internship.GraduationYear}");
            }

            return (true, null);
        }
    }
}
