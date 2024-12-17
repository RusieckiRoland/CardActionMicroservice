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

            return null; // Jeśli karta nie zostanie znaleziona
        }

        private static Dictionary<string, Dictionary<string, CardDetails>> CreateSampleUserCards()
        {
            return new Dictionary<string, Dictionary<string, CardDetails>>
        {
            {
                "User1", new Dictionary<string, CardDetails>
                {
                    { "Card123", new CardDetails("Card123", CardType.Prepaid, CardStatus.Active, true) },
                    { "Card124", new CardDetails("Card124", CardType.Debit, CardStatus.Ordered, false) }
                }
            },
            {
                "User2", new Dictionary<string, CardDetails>
                {
                    { "Card125", new CardDetails("Card125", CardType.Credit, CardStatus.Blocked, true) },
                    { "Card126", new CardDetails("Card126", CardType.Prepaid, CardStatus.Closed, false) }
                }
            }
        };
        }
    }

}
