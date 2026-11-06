using ECommerce.Application.DTOs.Newsletter;
using FluentValidation;

namespace ECommerce.Application.Validators.Newsletter;

public class SubscribeNewsletterValidator : AbstractValidator<SubscribeNewsletterDto>
{
    public SubscribeNewsletterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-poçt ünvanı mütləqdir.")
            .EmailAddress().WithMessage("Düzgün e-poçt ünvanı daxil edin.")
            .MaximumLength(256).WithMessage("E-poçt ünvanı 256 simvoldan çox ola bilməz.");
    }
}
