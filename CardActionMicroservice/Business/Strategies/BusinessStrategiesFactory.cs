using CardActionMicroservice.Infrastructure;

namespace CardActionMicroservice.Business.Strategies
{
    public static class BusinessStrategiesFactory
    {
        /// <summary>
        /// Creates a list of action strategies based on rules loaded from the IRuleLoader.
        /// </summary>
        /// <param name="ruleLoader">The rule loader providing card type, status, and pin rules.</param>
        /// <returns>List of IActionStrategy implementations.</returns>

        public static List<IActionStrategy> CreateStrategies(IRuleLoader ruleLoader)
        {
            var (cardTypeRules, cardStatusRules, pinRules) = ruleLoader.LoadRules();

            return new List<IActionStrategy>
        {
            new CardTypeStrategy(cardTypeRules),
            new CardStatusStrategy(cardStatusRules),
            new PinStrategy(pinRules)
        };
        }
    }

}
