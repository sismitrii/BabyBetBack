namespace Application.Dtos.Out.Stats;

public record WeightStatsDto
{
    public required double Weight { get; init; }
    public required int Count { get; init; }
    public required double Percentage { get; init; }

}