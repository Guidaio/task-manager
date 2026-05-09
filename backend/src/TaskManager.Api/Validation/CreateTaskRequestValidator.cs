using FluentValidation;
using TaskManager.Application.Dtos.Tasks;

namespace TaskManager.Api.Validation;

public sealed class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Description).MaximumLength(8000).When(x => x.Description is not null);
        RuleFor(x => x.Status).IsInEnum();
    }
}
