using Application.Dtos.Out;
using AutoMapper;
using Core.Interfaces;

namespace Application.Services;

public class BetGameService(IUnitOfWork unitOfWork, IMapper mapper) : IBetGameService
{
    public async Task<BetGameDto> FindByIdAsync(Guid betGameId)
    {
        var betGame = await unitOfWork.BetGameRepository.FindByIdAsync(betGameId) ??
                      throw new Exception($"No bet game found with this id {betGameId}");
        
        var betGameDto = mapper.Map<BetGameDto>(betGame);
        return betGameDto;
    }
}