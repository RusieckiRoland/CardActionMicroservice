using CardActionMicroservice.Models;
using FluentValidation;

namespace CardActionMicroservice.Validators
{

    public class CardRequestValidator : AbstractValidator<CardRequest>
    {
        public CardRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId cannot be empty.");

            RuleFor(x => x.CardNumber)
                .NotEmpty().WithMessage("CardNumber cannot be empty.");
        }
    }

}
