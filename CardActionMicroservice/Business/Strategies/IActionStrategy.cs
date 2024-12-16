using CardActionMicroservice.Models;

namespace CardActionMicroservice.Business.Strategies
{
    public interface IActionStrategy
    {
        bool IsApplicable(CardDetails cardDetails);
        IEnumerable<string> GetBlockedActions(CardDetails cardDetails);
    }
}
