using CardActionMicroservice.Models.Enums;
using CardActionMicroservice.Models;

namespace CardActionMicroservice.Business.Strategies
{
    public class PinStrategy : IActionStrategy
    {
        private readonly Dictionary<(bool IsPinSet, CardStatus), List<string>> _pinRules;

        public PinStrategy(Dictionary<(bool, CardStatus), List<string>> pinRules)
        {
            _pinRules = pinRules;
        }

        public bool IsApplicable(CardDetails cardDetails)
        {
            return _pinRules.ContainsKey((cardDetails.IsPinSet, cardDetails.CardStatus));
        }

        public IEnumerable<string> GetBlockedActions(CardDetails cardDetails)
      => _pinRules[(cardDetails.IsPinSet, cardDetails.CardStatus)];
    }
}
