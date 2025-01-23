using Application.Dtos.Out;
using Application.Exceptions;
using AutoMapper;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class BetGameService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<BetGameService> logger) : IBetGameService
{
    public async Task<BetGameDto> FindByIdAsync(Guid betGameId)
    {
        logger.LogDebug($"Searching bet game with id: {betGameId}");
        var betGame = await unitOfWork.BetGameRepository.FindByIdAsync(betGameId) ??
                      throw new BetGameException($"No bet game found with this id {betGameId}");
        
        var betGameDto = mapper.Map<BetGameDto>(betGame);
        return betGameDto;
    }
}