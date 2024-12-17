using CardActionMicroservice.DataProviders;
using Xunit;

namespace CardActionMicroservice.Tests.DataProvider
{
    public class InMemoryProviderTests
    {
        [Fact]
        public async Task GetCardDetails_Returns_Existing()
        {
            var provider = new InMemoryCardProvider();
            var details = await provider.GetCardDetails("User1", "Card11");

            Assert.NotNull(details);
        }



        [Theory]
        [InlineData("User1", "Card11", 0, 0, false)] // Prepaid, Ordered, NoPin
        [InlineData("User1", "Card14", 0, 3, true)]  // Prepaid, Restricted, WithPin
        [InlineData("User1", "Card18", 1, 0, true)]  // Debit, Ordered, WithPin
        [InlineData("User2", "Card219", 2, 4, false)]// Credit, Expired, NoPin
        [InlineData("User3", "Card317", 2, 2, false)]// Credit, Inactive, NoPin
        public async Task GetCardDetails_Returns_CorrectDetails(string user, string cardNumber, int expectedCardType, int expectedCardStatus, bool expectedIsPinSet)
        {
            // Arrange
            var provider = new InMemoryCardProvider();

            // Act
            var details = await provider.GetCardDetails(user, cardNumber);

            // Assert
            Assert.NotNull(details);
            Assert.Equal(cardNumber, details.CardNumber);
            Assert.Equal(expectedCardType, (int)details.CardType);
            Assert.Equal(expectedCardStatus, (int)details.CardStatus);
            Assert.Equal(expectedIsPinSet, details.IsPinSet);
        }
        [Fact]
        public async Task GetCardDetails_Returns_Null_ForEmpty()
        {
            var provider = new InMemoryCardProvider();
            var details = await provider.GetCardDetails("FakeUser", "Card11");

            Assert.Null(details);
        }
    }
}
