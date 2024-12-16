using CardActionMicroservice.Models;

namespace CardActionMicroservice.DataProvider
{
    public interface ICardProvider
    {
        Task<CardDetails?> GetCardDetails(string userId, string cardNumber);
    }
}
