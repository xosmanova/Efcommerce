using ECommerce.Application.DTOs.Users;
using FluentValidation;

namespace ECommerce.Application.Validators.Users;

public class CreateAdminUserValidator : AbstractValidator<CreateAdminUserDto>
{
    public CreateAdminUserValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad mütləqdir.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad mütləqdir.")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-poçt mütləqdir.")
            .EmailAddress().WithMessage("Düzgün e-poçt ünvanı daxil edin.")
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifrə mütləqdir.")
            .MinimumLength(6).WithMessage("Şifrə ən azı 6 simvol olmalıdır.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Səlahiyyət rolu seçilməlidir.");
    }
}
