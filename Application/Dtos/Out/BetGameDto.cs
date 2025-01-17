namespace Application.Dtos.Out;

public record BetGameDto
{
    public required string Name { get; init; }
    public DateTime DueTermDate { get; init; }
    public DateTime ClosingDate { get; init; } 
}