using CardActionMicroservice.Business.Services;
using CardActionMicroservice.Business.Strategies;
using CardActionMicroservice.DataProvider;
using CardActionMicroservice.Models.Enums;
using System.Text.Json;

namespace CardActionService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services, string jsonConfigPath)
        {
            var (cardTypeRules, cardStatusRules, pinRules) = LoadRulesFromConfig(jsonConfigPath);

            services.AddSingleton<IActionStrategy>(new CardTypeStrategy(cardTypeRules));
            services.AddSingleton<IActionStrategy>(new CardStatusStrategy(cardStatusRules));
            services.AddSingleton<IActionStrategy>(new PinStrategy(pinRules));
            services.AddSingleton<AllowedActionsService>();
            services.AddSingleton<ICardProvider, InMemoryCardProvider>();

            return services;
        }

        private static (Dictionary<CardType, List<string>>, Dictionary<CardStatus, List<string>>, Dictionary<(bool, CardStatus), List<string>>)
       LoadRulesFromConfig(string filePath)
        {
            var jsonConfig = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(filePath));

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


