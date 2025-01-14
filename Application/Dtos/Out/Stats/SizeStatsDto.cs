namespace Application.Dtos.Out.Stats;

public record SizeStatsDto
{
    public int Size { get; init; }
    public int Count { get; init; }
    public required double Percentage { get; init; }
}
