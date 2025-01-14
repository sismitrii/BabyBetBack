namespace Core.Entities;

public class BetGame : BaseEntity
{
    public required string Name { get; set; }
    public ICollection<Bet> Bets { get; set; } = new List<Bet>();
}