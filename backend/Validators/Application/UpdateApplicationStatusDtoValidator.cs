using FluentValidation;
using InternshipPortal.API.DTOs.Application;

namespace InternshipPortal.API.Validators.Application
{
    public class UpdateApplicationStatusDtoValidator
        : AbstractValidator<UpdateApplicationStatusDto>
    {
        public UpdateApplicationStatusDtoValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty()
                .Must(status =>
                    status == "Pending" ||
                    status == "Accepted" ||
                    status == "Rejected" ||
                    status == "Shortlisted")
                .WithMessage(
                    "Invalid application status"
                );

            RuleFor(x => x.AdminRemarks)
                .MaximumLength(1000);
        }
    }
}