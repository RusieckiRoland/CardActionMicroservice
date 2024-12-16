using CardActionMicroservice.Models.Enums;
using CardActionMicroservice.Models;

namespace CardActionMicroservice.Business.Strategies
{
    public class CardStatusStrategy : IActionStrategy
    {
        private readonly Dictionary<CardStatus, List<string>> _cardStatusRules;

        public CardStatusStrategy(Dictionary<CardStatus, List<string>> cardStatusRules)
        {
            _cardStatusRules = cardStatusRules;
        }

        public bool IsApplicable(CardDetails cardDetails) 
            => _cardStatusRules.ContainsKey(cardDetails.CardStatus);

        public IEnumerable<string> GetBlockedActions(CardDetails cardDetails) 
            => _cardStatusRules[cardDetails.CardStatus];
    }
}
