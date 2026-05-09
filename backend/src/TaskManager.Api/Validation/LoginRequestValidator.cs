using FluentValidation;
using TaskManager.Application.Dtos.Auth;

namespace TaskManager.Api.Validation;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(320).EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
