using CardActionMicroservice.Models;
using FluentValidation;

namespace CardActionMicroservice.Validators
{
    public class CardDetailsValidator : AbstractValidator<CardDetails>
    {
        public CardDetailsValidator()
        {
            RuleFor(card => card.CardNumber)
                .NotEmpty().WithMessage("Card number is required.");
            RuleFor(card => card.CardType)
                .IsInEnum().WithMessage("Invalid card type.");
            RuleFor(card => card.CardStatus)
                .IsInEnum().WithMessage("Invalid card status.");
            RuleFor(card => card.IsPinSet)
                .NotNull().WithMessage("Pin set status is required.");
        }
    }
}
