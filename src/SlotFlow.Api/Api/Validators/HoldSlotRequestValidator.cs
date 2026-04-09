using FluentValidation;

namespace SlotFlow.Api.Api.Validators;

public sealed record HoldSlotRequest(Guid SlotId);

public sealed class HoldSlotRequestValidator : AbstractValidator<HoldSlotRequest>
{
    public HoldSlotRequestValidator()
    {
        RuleFor(x => x.SlotId)
            .NotEmpty()
            .WithMessage("SlotId is required.");
    }
}