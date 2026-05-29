using FluentValidation;
using InternshipPortal.API.DTOs.Internship;

namespace InternshipPortal.API.Validators.Internship
{
    public class UpdateInternshipDtoValidator
        : AbstractValidator<UpdateInternshipDto>
    {
        public UpdateInternshipDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(5000);

            RuleFor(x => x.CompanyName)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Location)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Stipend)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.DurationInMonths)
                .GreaterThan(0);

            // ELIGIBILITY
            RuleFor(x => x.MinimumCGPA)
                .InclusiveBetween(0, 10);

            RuleFor(x => x.AllowedBacklogs)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.RequiredSkills)
                .NotEmpty();

            RuleFor(x => x.AllowedDepartments)
                .NotEmpty();

            RuleFor(x => x.GraduationYear)
                .InclusiveBetween(2020, 2100);

            // SEATS
            RuleFor(x => x.TotalSeats)
                .GreaterThan(0);

            // DATES
            RuleFor(x => x.StartDate)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date);

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate);

            RuleFor(x => x.RegistrationDeadline)
                .LessThanOrEqualTo(x => x.StartDate)
                .WithMessage(
                    "Registration deadline must be on or before internship start date"
                );
        }
    }
}