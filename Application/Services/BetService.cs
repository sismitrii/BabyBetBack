using Application.Dtos;
using Application.Dtos.In;
using Application.Dtos.Out;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using FluentValidation;

namespace Application.Services;

public class BetService(IValidator<CreateUserBetRequest> createUserBetRequestValidator, IMapper mapper, IUnitOfWork unitOfWork)
    : IBetService
{
    public async Task<BetDto> CreateBetAsync(CreateUserBetRequest betRequest, string? userIdentifier)
    {
        var user = await unitOfWork.UserRepository.FindUserByEmailAsync(userIdentifier) ??
            throw new Exception($"User with email {userIdentifier} does not exist"); // to refacto with a good exception

        var betGame = await unitOfWork.BetGameRepository.FindByIdAsync(betRequest.BetGameId) ??
            throw new Exception($"BetGame invalid"); // to refacto with a good exception
            
        var betForUserAlreadyExisting = await unitOfWork.BetRepository.ExistBetForUserAsync(user);
        
        if (betForUserAlreadyExisting)
            throw new Exception($"A bet is already existing for this user");
        
        await createUserBetRequestValidator.ValidateAndThrowAsync(betRequest);
        
        var bet = mapper.Map<Bet>(betRequest);
        bet.User = user;

        betGame.Bets.Add(bet);
        await unitOfWork.SaveChangesAsync();
        
        var betDto = mapper.Map<BetDto>(bet);
        return betDto;
    }

    public async Task<BetDto> FindByIdAsync(Guid betId)
    {
        var bet = await unitOfWork.BetRepository.FindByIdAsync(betId) ??
            throw new Exception($"Bet with id {betId} does not exist"); //TODO configure exception
        
        var betDto = mapper.Map<BetDto>(bet);
        return betDto;
    }

    public async Task<BetDto?> FindBetOfUserAsync(string? getNameIdentifierId)
    {
        if (getNameIdentifierId == null)
            throw new Exception("Name Identifier is mandatory");
        
        var bet = await unitOfWork.BetRepository.FindByUserIdentifier(getNameIdentifierId);
        
        var betDto = mapper.Map<BetDto>(bet);
        return betDto;
    }

    public async Task<IEnumerable<BetDto>> GetAllForAGameAsync(Guid betGameId)
    {
        var betGame = await unitOfWork.BetGameRepository.FindByIdAsync(betGameId) ?? 
                      throw new Exception($"No BetGame found with id {betGameId}");
        
        var betsDto = betGame.Bets.Select(bet => mapper.Map<BetDto>(bet));
        return betsDto;
    }

    public async Task<IEnumerable<BetGame>> GetUser()
    {
        return await unitOfWork.BetGameRepository.GetAllAsync();
    }

    public async Task DeleteAsync(Guid betId)
    {
        await unitOfWork.BetRepository.DeleteByIdAsync(betId);
        await unitOfWork.SaveChangesAsync();
    }
}