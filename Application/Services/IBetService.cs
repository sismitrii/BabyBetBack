using Application.Dtos;
using Application.Dtos.In;
using Application.Dtos.Out;
using Core.Entities;

namespace Application.Services;

public interface IBetService
{
    Task<BetDto> CreateBetAsync(CreateUserBetRequest betDto, string? userIdentifier);
    Task<BetDto> FindByIdAsync(Guid betId);
    Task<BetDto?> FindBetOfUserAsync(string? getNameIdentifierId);
    Task<IEnumerable<BetDto>> GetAllForAGameAsync(Guid betGameId);

    Task<IEnumerable<BetGame>> GetUser();

    Task UpdateAsync(Guid betId, UpdateUserBetRequest betDto);
    Task DeleteAsync(Guid betId);
}