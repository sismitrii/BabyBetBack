namespace Core.Entities;

public class BetGame : BaseEntity
{
    public required string Name { get; set; }
    public ICollection<Bet> Bets { get; set; } = new List<Bet>();
    public Result Result { get; set; }
    public DateTime DueTermDate { get; set; } // TODO : set required
    public DateTime ClosingDate { get; set; } // TOOO : set required
}