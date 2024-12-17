using CardActionMicroservice.Models.Enums;
using System.Text.Json;

namespace CardActionMicroservice.Infrastructure
{
        public class JsonRuleLoader : IRuleLoader
    {
        private readonly string _jsonContent;

        public JsonRuleLoader(string jsonContent)
        {
            _jsonContent = jsonContent;
        }

        public (Dictionary<CardType, List<string>>, Dictionary<CardStatus, List<string>>, Dictionary<(bool, CardStatus), List<string>>)
        LoadRules()
        {
            // Deserializacja JSON-a
            var jsonConfig = JsonSerializer.Deserialize<JsonElement>(_jsonContent);

            var cardTypeRules = jsonConfig.GetProperty("CardTypeRules")
                .Deserialize<Dictionary<CardType, List<string>>>() ?? new Dictionary<CardType, List<string>>();

            var cardStatusRules = jsonConfig.GetProperty("CardStatusRules")
                .Deserialize<Dictionary<CardStatus, List<string>>>() ?? new Dictionary<CardStatus, List<string>>();

            var pinRules = jsonConfig.GetProperty("PinRules")
                .Deserialize<Dictionary<string, Dictionary<string, List<string>>>>() ?? new Dictionary<string, Dictionary<string, List<string>>>();

            var pinConvertedRules = new Dictionary<(bool, CardStatus), List<string>>();
            foreach (var pinRule in pinRules)
            {
                bool isPinSet = pinRule.Key == "WithPin";
                foreach (var statusRule in pinRule.Value)
                {
                    if (Enum.TryParse<CardStatus>(statusRule.Key, out var status))
                    {
                        pinConvertedRules[(isPinSet, status)] = statusRule.Value;
                    }
                }
            }

            return (cardTypeRules, cardStatusRules, pinConvertedRules);
        }
    }


}
