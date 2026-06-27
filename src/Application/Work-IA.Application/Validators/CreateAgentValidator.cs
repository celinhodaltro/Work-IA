using FluentValidation;
using Work_IA.Application.Agents.Commands;

namespace Work_IA.Application.Validators;

public sealed class CreateAgentValidator : AbstractValidator<CreateAgentCommand>
{
    public CreateAgentValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(v => v.Title)
            .NotEmpty()
            .MaximumLength(200);
    }
}
