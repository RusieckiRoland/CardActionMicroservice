using CardActionMicroservice.Models.Enums;
using CardActionMicroservice.Models;

namespace CardActionMicroservice.DataProviders
{
    public class InMemoryCardProvider : ICardProvider
    {
        private readonly Dictionary<string, Dictionary<string, CardDetails>> _userCards;

        public InMemoryCardProvider()
        {
            _userCards = CreateSampleUserCards();
        }

        public async Task<CardDetails?> GetCardDetails(string userId, string cardNumber)
        {
            // Symulacja opóźnienia (jak przy wywołaniu API lub bazy danych)
            await Task.Delay(100);

            if (_userCards.TryGetValue(userId, out var cards) &&
                cards.TryGetValue(cardNumber, out var cardDetails))
            {
                return cardDetails;
            }

            return null; 
        }

        private static Dictionary<string, Dictionary<string, CardDetails>> CreateSampleUserCards()
        {
            var userCards = new Dictionary<string, Dictionary<string, CardDetails>>();
            for (var i = 1; i <= 3; i++)
            {
                var cards = new Dictionary<string, CardDetails>();
                var cardIndex = 1;
                foreach (CardType cardType in Enum.GetValues(typeof(CardType)))
                {
                    foreach (CardStatus cardStatus in Enum.GetValues(typeof(CardStatus)))
                    {
                        var cardNumber = $"Card{i}{cardIndex}";
                        cards.Add(cardNumber,
                        new CardDetails(
                        CardNumber: cardNumber,
                        CardType: cardType,
                        CardStatus: cardStatus,
                        IsPinSet: cardIndex % 2 == 0));
                        cardIndex++;
                    }
                }
                var userId = $"User{i}";
                userCards.Add(userId, cards);
            }
            return userCards;
        }

    }

}
