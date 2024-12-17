using CardActionMicroservice.Business.Services;
using CardActionMicroservice.Models.Enums;
using CardActionMicroservice.Models;
using Xunit;
using FluentAssertions;
using System.Text.Json;
using CardActionMicroservice.Infrastructure;
using CardActionMicroservice.Business.Strategies;
using System.Text;


namespace CardActionMicroservice.Tests.Business.Services
{
    public class AllowedActionsServiceTests
    {
        [Fact]
        public void GetAllowedActions_ReturnsCorrectActions_BasedOnJsonStrategiesAndCards()
        {
            // Arrange
            var jsonContent = GetJsonContentForFirstConfiguration();
            var ruleLoader = new JsonRuleLoader(jsonContent);            

            var allowedActionService = new AllowedActionsService(ruleLoader);

            var cardDetails = new CardDetails("1234567890123456", CardType.Prepaid, CardStatus.Ordered, false);            

            // Act
            var allowedActions = allowedActionService.GetAllowedActions(cardDetails);



            // Assert
            var allActions = JsonSerializer.Deserialize<JsonElement>(jsonContent)
                                   .GetProperty("AllActions")
                                   .Deserialize<List<string>>();
            //Na podstawie tabeli
            var blockedActions = new List<string> { "ACTION1", "ACTION2", "ACTION5", "ACTION6","ACTION11" };                       
          
            var expectedActions = allActions?.Except(blockedActions);

            allowedActions.Should().BeEquivalentTo(expectedActions);
        }

        private string GetJsonContentForFirstConfiguration()
        {
            var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Config", "ActionsConfiguration_ver1.json");
            var jsonContent = File.ReadAllText(configFilePath, Encoding.UTF8);
            return jsonContent;

        }
    }

}
