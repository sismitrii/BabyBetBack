using Application.Dtos.Out;

namespace Application.Services;

public interface IBetGameService
{
    Task<BetGameDto> FindByIdAsync(Guid betGameId);
}