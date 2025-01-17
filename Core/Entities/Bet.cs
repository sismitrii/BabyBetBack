using Core.Enum;

namespace Core.Entities;

public class Bet : BaseEntity
{
    public Gender Gender { get; set; }
    public DateOnly BirthDate { get; set; }
    public TimeOnly BirthTime { get; set; }
    public int Size { get; set; }
    public double Weight { get; set; }
    public IEnumerable<Name> Names { get; set; } = new List<Name>();
    public string? AdditionalMessage { get; set; }
    public required string NameByUser { get; set; }
    public required User User { get; set; }
    public required DateTime CreatedAt { get; set; } = DateTime.Now;
}