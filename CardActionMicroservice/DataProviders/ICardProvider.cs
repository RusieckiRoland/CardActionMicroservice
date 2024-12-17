using CardActionMicroservice.Models;

namespace CardActionMicroservice.DataProviders
{
    public interface ICardProvider
    {

        /// <summary>
        /// Retrieves card details for a specified user and card number.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="cardNumber">The number of the card.</param>
        /// <returns>
        /// The details of the card if found; otherwise, null.
        /// </returns>
        /// <remarks>Simulates a database call with Task.Delay for testing purposes.</remarks>
        Task<CardDetails?> GetCardDetails(string userId, string cardNumber);
    }
}
