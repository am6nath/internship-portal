using FluentValidation;
using InternshipPortal.API.DTOs.Student;

namespace InternshipPortal.API.Validators.Student
{
    public class UpdateStudentProfileDtoValidator
        : AbstractValidator<UpdateStudentProfileDto>
    {
        public UpdateStudentProfileDtoValidator()
        {
            RuleFor(x => x.CollegeName)
                .NotEmpty()
                .WithMessage("College Name is required")
                .MaximumLength(200);

            RuleFor(x => x.Department)
                .NotEmpty()
                .WithMessage("Department is required")
                .MaximumLength(100);

            RuleFor(x => x.CGPA)
                .InclusiveBetween(0, 10)
                .WithMessage("CGPA must be between 0 and 10");

            RuleFor(x => x.Backlogs)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Backlogs cannot be negative");

            RuleFor(x => x.GraduationYear)
                .InclusiveBetween(2020, 2100)
                .WithMessage("Invalid Graduation Year");

            RuleFor(x => x.Skills)
                .NotEmpty()
                .WithMessage("Skills are required");
        }
    }
}