using CardActionMicroservice.Business.Strategies;
using CardActionMicroservice.Infrastructure;
using CardActionMicroservice.Models;

namespace CardActionMicroservice.Business.Services
{
    public class AllowedActionsService
    {
        private readonly IEnumerable<IActionStrategy> _strategies;

        public AllowedActionsService(IRuleLoader ruleLoader)
        {
            _strategies = BusinessStrategiesFactory.CreateStrategies(ruleLoader);
        }

        /// <summary>
        /// Determines the list of allowed actions for a card by applying a set of strategies.
        /// </summary>
        /// <param name="cardDetails">Details of the card including status, type, and pin state.</param>
        /// <returns>A list of allowed actions after filtering out blocked actions.</returns>


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
