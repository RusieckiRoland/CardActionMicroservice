using CardActionMicroservice.Business.Services;
using CardActionMicroservice.Models.Enums;
using CardActionMicroservice.Models;
using Xunit;
using FluentAssertions;
using System.Text.Json;
using CardActionMicroservice.Infrastructure;
using CardActionMicroservice.Business.Strategies;
using System.Runtime.CompilerServices;

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
            var strategyList = BusinessStrategiesFactory.CreateStrategies(ruleLoader);

            var allowedActionService = new AllowedActionsService(strategyList);

            var cardDetails = new CardDetails("1234567890123456", CardType.Prepaid, CardStatus.Ordered, false);            

            // Act
            var allowedActions = allowedActionService.GetAllowedActions(cardDetails);



            // Assert
            var allActions = JsonSerializer.Deserialize<JsonElement>(jsonContent)
                                   .GetProperty("AllActions")
                                   .Deserialize<List<string>>();
            //Na podstawie tabeli
            var blockedActions = new List<string> { "ACTION1", "ACTION2", "ACTION5", "ACTION6" };                       
          
            var expectedActions = allActions?.Except(blockedActions);

            allowedActions.Should().BeEquivalentTo(expectedActions);
        }

        private string GetJsonContentForFirstConfiguration()
        {
            return @"{
           ""AllActions"": [
             ""ACTION1"",
             ""ACTION2"",
             ""ACTION3"",
             ""ACTION4"",
             ""ACTION5"",
             ""ACTION6"",
             ""ACTION7"",
             ""ACTION8"",
             ""ACTION9"",
             ""ACTION10"",
             ""ACTION11"",
             ""ACTION12"",
             ""ACTION13""
           ],
           ""CardTypeRules"": {
               ""Prepaid"": [ ""ACTION5"" ],
               ""Debit"": [ ""ACTION6"" ],
               ""Credit"": []
           },
           ""CardStatusRules"": {
               ""Ordered"": [ ""ACTION1"", ""ACTION2"" ],
               ""Inactive"": [ ""ACTION1"" ],
               ""Active"": [ ""ACTION2"" ],
               ""Restricted"": [ ""ACTION1"", ""ACTION2"", ""ACTION6"", ""ACTION7"" ],
               ""Blocked"": [ ""ACTION6"", ""ACTION7"" ],
               ""Expired"": [ ""ACTION10"", ""ACTION11"", ""ACTION12"", ""ACTION13"" ],
               ""Closed"": [ ""ACTION10"", ""ACTION11"", ""ACTION12"", ""ACTION13"" ]
           },
           ""PinRules"": {
              ""NoPin"": {
                ""Ordered"": [ ""ACTION6"" ],
                ""Inactive"": [ ""ACTION6"" ],
                ""Active"": [ ""ACTION6"" ]
              },
              ""WithPin"": {
                ""Ordered"": [ ""ACTION7"" ],
                ""Blocked"": [ ""ACTION6"", ""ACTION7"" ]
             }
            }
           }";

        }
    }

}
