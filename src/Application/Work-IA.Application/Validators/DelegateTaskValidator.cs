using FluentValidation;
using Work_IA.Application.Agents.Commands;

namespace Work_IA.Application.Validators;

public sealed class DelegateTaskValidator : AbstractValidator<DelegateTaskCommand>
{
    public DelegateTaskValidator()
    {
        RuleFor(v => v.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(v => v.Description)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(v => v.TargetLevel)
            .IsInEnum();
    }
}
