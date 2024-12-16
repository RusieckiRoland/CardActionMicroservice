using CardActionMicroservice.Models;
using CardActionMicroservice.Models.Enums;

namespace CardActionMicroservice.Business.Strategies
{
    public class CardTypeStrategy : IActionStrategy
    {
        private readonly Dictionary<CardType, List<string>> _cardTypeRules;

        public CardTypeStrategy(Dictionary<CardType, List<string>> cardTypeRules)
        {
            _cardTypeRules = cardTypeRules;
        }

        public bool IsApplicable(CardDetails cardDetails) => 
            _cardTypeRules.ContainsKey(cardDetails.CardType);

       

        public IEnumerable<string> GetBlockedActions(CardDetails cardDetails) =>
            _cardTypeRules[cardDetails.CardType];
    }
}
