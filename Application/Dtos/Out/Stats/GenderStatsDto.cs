namespace Application.Dtos.Out.Stats;

public record GenderStatsDto
{
    public double BoyPercentage { get; init; }
    public double GirlPercentage { get; init; }
}