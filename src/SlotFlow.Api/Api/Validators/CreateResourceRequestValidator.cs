using FluentValidation;

namespace SlotFlow.Api.Api.Validators;

public sealed record CreateResourceRequest(
    string Name,
    string? Description,
    int HoldDurationMinutes,
    int InitialSlotCount);

public sealed class CreateResourceRequestValidator : AbstractValidator<CreateResourceRequest>
{
    public CreateResourceRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.HoldDurationMinutes)
            .InclusiveBetween(1, 1440)
            .WithMessage("Hold duration must be between 1 and 1440 minutes.");

        RuleFor(x => x.InitialSlotCount)
            .InclusiveBetween(1, 500)
            .WithMessage("Initial slot count must be between 1 and 500.");
    }
}