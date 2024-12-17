using CardActionMicroservice.Infrastructure;

namespace CardActionMicroservice.Business.Strategies
{
    public static class BusinessStrategiesFactory
    {
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
