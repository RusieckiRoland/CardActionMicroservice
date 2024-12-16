using CardActionMicroservice.Business.Strategies;
using CardActionMicroservice.Models;

namespace CardActionMicroservice.Business.Services
{
    public class AllowedActionsService
    {
        private readonly IEnumerable<IActionStrategy> _strategies;

        public AllowedActionsService(IEnumerable<IActionStrategy> strategies)
        {
            _strategies = strategies;
        }

        public IEnumerable<string> GetAllowedActions(CardDetails cardDetails)
        {
            var blockedActions = _strategies
                .Where(strategy => strategy.IsApplicable(cardDetails))
                .SelectMany(strategy => strategy.GetBlockedActions(cardDetails))
                .Distinct();

            var allActions = Enumerable.Range(1, 13).Select(i => $"ACTION{i}");
            return allActions.Except(blockedActions);
        }
    }
}
