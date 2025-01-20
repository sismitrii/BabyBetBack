namespace Application.Dtos.In;

public record LoginRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}