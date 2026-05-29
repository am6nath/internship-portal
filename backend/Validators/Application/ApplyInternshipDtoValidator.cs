using FluentValidation;
using InternshipPortal.API.DTOs.Application;

namespace InternshipPortal.API.Validators.Application
{
    public class ApplyInternshipDtoValidator
        : AbstractValidator<ApplyInternshipDto>
    {
        public ApplyInternshipDtoValidator()
        {
            RuleFor(x => x.InternshipId)
                .NotEmpty()
                .WithMessage(
                    "Internship Id is required"
                );
        }
    }
}