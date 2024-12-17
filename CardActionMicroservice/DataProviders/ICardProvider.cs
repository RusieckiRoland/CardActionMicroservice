using CardActionMicroservice.Models;

namespace CardActionMicroservice.DataProviders
{
    public interface ICardProvider
    {
        Task<CardDetails?> GetCardDetails(string userId, string cardNumber);
    }
}
