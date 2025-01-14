namespace Application.Dtos.Out.Stats;

public record BirthDateStatsDto
{
    public DateOnly Date { get; init; }
    public int Count { get; init; }
}