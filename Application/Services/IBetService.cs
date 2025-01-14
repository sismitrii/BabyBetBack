using Application.Dtos;
using Application.Dtos.In;
using Application.Dtos.Out;

namespace Application.Services;

public interface IBetService
{
    Task<BetDto> CreateBetAsync(CreateUserBetRequest betDto, string? userIdentifier);
    Task<BetDto> FindByIdAsync(Guid betId);
    Task<BetDto?> FindBetOfUserAsync(string? getNameIdentifierId);
    Task<IEnumerable<BetDto>> GetAllForAGameAsync(Guid betGameId);
    
    Task DeleteAsync(Guid betId);
}