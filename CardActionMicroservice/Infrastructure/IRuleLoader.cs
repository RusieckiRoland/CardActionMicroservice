using CardActionMicroservice.Models.Enums;

namespace CardActionMicroservice.Infrastructure
{
    public interface IRuleLoader
    {
        (Dictionary<CardType, List<string>> cardTypeRules, Dictionary<CardStatus,
            List<string>> cardStatusRules, Dictionary<(bool, CardStatus), List<string>> pinRules) LoadRules();
    }
}
