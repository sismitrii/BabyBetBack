using Core.Enum;

namespace Core.Entities;

public class Result : BaseEntity
{
    public Guid BetGameId { get; set; }
    public Gender? Gender { get; set; }
    public DateOnly? BirthDate { get; set; }
    public TimeOnly? BirthTime { get; set; }
    public int? Size { get; set; }
    public double? Weight { get; set; }
    public string? Name { get; set; }
}