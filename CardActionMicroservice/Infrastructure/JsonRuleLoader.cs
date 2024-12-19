using CardActionMicroservice.Models.Enums;
using System.Text.Json;

namespace CardActionMicroservice.Infrastructure
{
        public class JsonRuleLoader : IRuleLoader
    {
        private readonly string _jsonContent;
        private readonly JsonElement _jsonConfig;

        public JsonRuleLoader(string jsonContent)
        {
            _jsonContent = jsonContent;
            _jsonConfig = JsonSerializer.Deserialize<JsonElement>(_jsonContent);
        }

        public HashSet<string> LoadAllAvailableActions()
        {
            try
            {
                return _jsonConfig
                    .GetProperty("AllActions")
                    .EnumerateArray()
                    .Select(action => action.GetString())
                    .Where(action => action != null) 
                    .Select(action => action!)      
                    .ToHashSet();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas ładowania dostępnych akcji: {ex.Message}");
                return new HashSet<string>();
            }
        }




        /// <summary>
        /// Loads rules for card type, status, and pin from JSON content.
        /// </summary>
        /// <returns>
        /// Tuple containing:  
        /// - Card type rules.  
        /// - Card status rules.  
        /// - Pin rules (converted to a structured format).
        /// </returns>


        public (Dictionary<CardType, List<string>>, Dictionary<CardStatus, List<string>>, 
               Dictionary<(bool, CardStatus), List<string>>)
        LoadRules()
        {
            var cardTypeRules = new Dictionary<CardType, List<string>>();
            var cardStatusRules = new Dictionary<CardStatus, List<string>>();
            var pinConvertedRules = new Dictionary<(bool, CardStatus), List<string>>();
            try
            {
                 cardTypeRules = _jsonConfig.GetProperty("CardTypeRules")
                    .Deserialize<Dictionary<CardType, List<string>>>() ?? new Dictionary<CardType, List<string>>();

                 cardStatusRules = _jsonConfig.GetProperty("CardStatusRules")
                    .Deserialize<Dictionary<CardStatus, List<string>>>() ?? new Dictionary<CardStatus, List<string>>();

                var pinRules = _jsonConfig.GetProperty("PinRules")
                    .Deserialize<Dictionary<string, Dictionary<string, List<string>>>>() ?? new Dictionary<string, Dictionary<string, List<string>>>();

                pinConvertedRules = new Dictionary<(bool, CardStatus), List<string>>();
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

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas ładowania restrykcji: {ex.Message}");
            }

            return (cardTypeRules, cardStatusRules, pinConvertedRules);
        }
    }


}
