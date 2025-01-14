namespace Application.Dtos.Out.Stats;

public record StatsDto
{
    public required GenderStatsDto GenderStats { get; init; }
    public required IEnumerable<BirthDateStatsDto> BirthDateStats { get; init; } = new List<BirthDateStatsDto>();
    public required IEnumerable<SizeStatsDto> SizeStats { get; init; } = new List<SizeStatsDto>();
    public required IEnumerable<WeightStatsDto> WeightStats { get; init; } = new List<WeightStatsDto>();
    public required IEnumerable<NameStatsDto> NamesStats { get; init; } = new List<NameStatsDto>();
}