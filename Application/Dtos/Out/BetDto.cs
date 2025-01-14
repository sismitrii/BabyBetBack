using Application.Dtos.Common;
using Core.Enum;

namespace Application.Dtos.Out;

public record BetDto
{
    public Guid Id { get; init; }
    public Gender Gender { get; init; }
    public DateOnly BirthDate { get; init; }
    public TimeOnly BirthTime { get; init; }
    public int Size { get; init; }
    public double Weight { get; init; }
    public IEnumerable<NameDto> Names { get; init; } = new List<NameDto>();
    public required string NameByUser { get; init; }
    public string? AdditionalMessage { get; init; }
    public required UserDto User { get; init; }
}