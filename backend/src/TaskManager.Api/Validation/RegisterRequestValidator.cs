using FluentValidation;
using TaskManager.Application.Dtos.Auth;

namespace TaskManager.Api.Validation;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(320).EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
