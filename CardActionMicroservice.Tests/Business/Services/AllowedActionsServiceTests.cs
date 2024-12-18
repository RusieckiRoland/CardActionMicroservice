using CardActionMicroservice.Business.Services;
using CardActionMicroservice.Models.Enums;
using CardActionMicroservice.Models;
using Xunit;
using FluentAssertions;
using System.Text.Json;
using CardActionMicroservice.Infrastructure;
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
            var blockedActions = new List<string> { "ACTION1", "ACTION2", "ACTION5", "ACTION6", "ACTION11" };

            var expectedActions = allActions?.Except(blockedActions);

            allowedActions.Should().BeEquivalentTo(expectedActions);
        }

        [Theory]
        [MemberData(nameof(GetRepresentativeTestCases))]
        public void GetAllowedActions_ReturnsCorrectActions_ForRepresentativeTestCases(
       CardDetails cardDetails, List<string> expectedActions)
        {
            // Arrange
            var jsonContent = GetJsonContentForFirstConfiguration();
            var ruleLoader = new JsonRuleLoader(jsonContent);
            var allowedActionService = new AllowedActionsService(ruleLoader);

            // Act
            var allowedActions = allowedActionService.GetAllowedActions(cardDetails);

            // Assert
            try
            {
                allowedActions.Should().BeEquivalentTo(expectedActions);
                //  LogToFile($"[PASS] CardDetails: {cardDetails}, AllowedActions: {string.Join(", ", allowedActions)}");
            }
            catch (Exception)
            {
                //   LogToFile($"[FAIL] CardDetails: {cardDetails}, Expected: {string.Join(", ", expectedActions)}, Actual: {string.Join(", ", allowedActions)}");
                throw;
            }
        }


        public static IEnumerable<object[]> GetRepresentativeTestCases =>
    new List<object[]>
    {
        new object[]
        {
            new CardDetails("1234567890123456", CardType.Credit, CardStatus.Ordered, true),
            new List<string> {  "ACTION3", "ACTION4", "ACTION5",  "ACTION6", "ACTION8", "ACTION9", "ACTION10",  "ACTION12", "ACTION13" }

        },
        new object[]
        {
            new CardDetails("9876543210987654", CardType.Credit, CardStatus.Ordered, false),
            new List<string> {  "ACTION3", "ACTION4", "ACTION5", "ACTION7", "ACTION8", "ACTION9", "ACTION10", "ACTION12", "ACTION13" }

        },
        new object[]
        {
            new CardDetails("1111222233334444", CardType.Credit, CardStatus.Inactive, true),
            new List<string> {  "ACTION2", "ACTION3", "ACTION4", "ACTION5", "ACTION6",  "ACTION8", "ACTION9", "ACTION10", "ACTION11", "ACTION12", "ACTION13" }

        },
         new object[]
        {
            new CardDetails("1234567890123456", CardType.Credit, CardStatus.Inactive, true),
            new List<string> {  "ACTION2", "ACTION3", "ACTION4", "ACTION5", "ACTION6",  "ACTION8", "ACTION9", "ACTION10", "ACTION11", "ACTION12", "ACTION13" }

        },
        new object[]
        {
            new CardDetails("9876543210987654", CardType.Credit, CardStatus.Active, true),
            new List<string> { "ACTION1", "ACTION3", "ACTION4", "ACTION5", "ACTION6", "ACTION8", "ACTION9", "ACTION10", "ACTION11", "ACTION12", "ACTION13" }

        },
        new object[]
        {
            new CardDetails("9876543210987654", CardType.Credit, CardStatus.Active, false),
            new List<string> { "ACTION1", "ACTION3", "ACTION4", "ACTION5",  "ACTION7", "ACTION8", "ACTION9", "ACTION10", "ACTION11", "ACTION12", "ACTION13" }

        },
        new object[]
        {
            new CardDetails("1111222233334444", CardType.Credit, CardStatus.Restricted, true),
            new List<string> {  "ACTION3", "ACTION4", "ACTION5",  "ACTION9" }

        },
         new object[]
        {
            new CardDetails("1234567890123456", CardType.Credit, CardStatus.Blocked, true),
            new List<string> {  "ACTION3", "ACTION4", "ACTION5", "ACTION6", "ACTION7", "ACTION8", "ACTION9" }

        },
         new object[]
        {
            new CardDetails("1234567890123456", CardType.Credit, CardStatus.Blocked,false),
            new List<string> {  "ACTION3", "ACTION4", "ACTION5",  "ACTION8", "ACTION9" }

        },
        new object[]
        {
            new CardDetails("9876543210987654", CardType.Credit, CardStatus.Expired, true),
            new List<string> { "ACTION3", "ACTION4", "ACTION5", "ACTION9" }

        },
        new object[]
        {
            new CardDetails("9876543210987654", CardType.Credit, CardStatus.Expired, false),
            new List<string> { "ACTION3", "ACTION4", "ACTION5", "ACTION9" }

        },
        new object[]
        {
            new CardDetails("1111222233334444", CardType.Debit, CardStatus.Closed, true),
            new List<string> { "ACTION3", "ACTION4", "ACTION9" }

        },
         new object[]
        {
            new CardDetails("1111222233334444", CardType.Debit, CardStatus.Closed, false),
            new List<string> { "ACTION3", "ACTION4", "ACTION9" }

        },
          new object[]
        {
            new CardDetails("9876543210987654", CardType.Prepaid, CardStatus.Ordered, false),
            new List<string> {  "ACTION3", "ACTION4",  "ACTION7", "ACTION8", "ACTION9", "ACTION10", "ACTION12", "ACTION13" }

        },
           new object[]
        {
            new CardDetails("1234567890123456", CardType.Debit, CardStatus.Blocked, true),
            new List<string> {  "ACTION3", "ACTION4",  "ACTION6", "ACTION7", "ACTION8", "ACTION9" }

        },
    };



        private string GetJsonContentForFirstConfiguration()
        {
            var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Config", "ActionsConfiguration_ver1.json");
            var jsonContent = File.ReadAllText(configFilePath, Encoding.UTF8);
            return jsonContent;

        }
        /// <summary>
        /// Logs a message to the "TestResults.log" file.
        /// This method is used to record the outcomes of test cases or debug information.
        /// The calls to this method are currently commented out but left in place 
        /// in case test execution or debugging requires detailed logging in the future.
        /// </summary>
        /// <param name="message">The message to log. Typically includes test case details or outcomes.</param>
        public static void LogToFile(string message)
        {
            var logFilePath = "TestResults.log";
            File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}{Environment.NewLine}");
        }
    }

}
