using CardActionMicroservice.Models.Enums;


namespace CardActionMicroservice.Models
{
    public record CardDetails(string CardNumber, CardType CardType, CardStatus CardStatus, bool IsPinSet);
}
