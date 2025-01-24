using Application.Dtos.In;
using Application.Dtos.Out;
using Application.Exceptions;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class BetService(
    IValidator<CreateUserBetRequest> createUserBetRequestValidator,
    IMapper mapper,
    IUnitOfWork unitOfWork,
    ILogger<BetService> logger)
    : IBetService
{
    public async Task<BetDto> CreateBetAsync(CreateUserBetRequest betRequest, string? userIdentifier)
    {
        var user = await unitOfWork.UserRepository.FindUserByEmailAsync(userIdentifier) ??
            throw new UserNotFoundException($"User with email {userIdentifier} does not exist"); // to refacto with a good exception

        var betGame = await unitOfWork.BetGameRepository.FindByIdAsync(betRequest.BetGameId) ??
            throw new BetGameException($"BetGame invalid"); // to refacto with a good exception
            
        var betForUserAlreadyExisting = await unitOfWork.BetRepository.ExistBetForUserAsync(user);
        
        if (betForUserAlreadyExisting)
            throw new BetException($"A bet is already existing for this user");
        
        await createUserBetRequestValidator.ValidateAndThrowAsync(betRequest);
        
        var bet = mapper.Map<Bet>(betRequest);
        bet.User = user;
        bet.CreatedAt = DateTime.Now;

        betGame.Bets.Add(bet);
        await unitOfWork.SaveChangesAsync();
        
        logger.LogDebug($"New bet added to Bet Game '{betGame.Name}' : {bet}");
        
        var betDto = mapper.Map<BetDto>(bet);
        return betDto;
    }

    public async Task<BetDto> FindByIdAsync(Guid betId)
    {
        var bet = await unitOfWork.BetRepository.FindByIdAsync(betId) ??
            throw new BetException($"Bet with id {betId} does not exist"); //TODO configure exception
        
        logger.LogDebug($"Bet with id {betId} found : {bet}");
        var betDto = mapper.Map<BetDto>(bet);
        return betDto;
    }

    public async Task<BetDto?> FindBetOfUserAsync(string? getNameIdentifierId)
    {
        if (getNameIdentifierId == null)
            throw new UserNotFoundException("Name Identifier is mandatory");
        
        var bet = await unitOfWork.BetRepository.FindByUserIdentifier(getNameIdentifierId);
        
        logger.LogDebug($"Bet for user '{getNameIdentifierId}' found : {bet}");
        
        var betDto = mapper.Map<BetDto>(bet);
        return betDto;
    }

    public async Task<IEnumerable<BetDto>> GetAllForAGameAsync(Guid betGameId)
    {
        logger.LogDebug($"Getting all bets for {betGameId}");
        var betGame = await unitOfWork.BetGameRepository.FindByIdAsync(betGameId) ?? 
                      throw new BetGameException($"No BetGame found with id {betGameId}");
        
        var betsDto = betGame.Bets.Select(bet => mapper.Map<BetDto>(bet));
        return betsDto;
    }

    public async Task UpdateAsync(Guid betId, UpdateUserBetRequest request)
    {
        logger.LogDebug($"Updating bet with id {betId}");
        var bet = await unitOfWork.BetRepository.FindByIdAsync(betId) ??
                  throw new BetException($"Bet with id {betId} does not exist");
        
        if (request.Gender.HasValue)
            bet.Gender = request.Gender.Value;
        
        if(request.BirthDate.HasValue)
            bet.BirthDate = request.BirthDate.Value;
        
        if (request.BirthTime.HasValue)
            bet.BirthTime = request.BirthTime.Value;
        
        if (request.Size.HasValue)
            bet.Size = request.Size.Value;

        if (request.Weight.HasValue)
            bet.Weight = request.Weight.Value;

        if (!string.IsNullOrWhiteSpace(request.NameByUser))
            bet.NameByUser = request.NameByUser;
        
        if (!string.IsNullOrWhiteSpace(request.AdditionalMessage))
            bet.AdditionalMessage = request.AdditionalMessage;

        if (request.Names?.Any() == true)
            bet.Names = request.Names
                .Select(mapper.Map<Name>)
                .ToList();

        logger.LogDebug($"Updated bet before saving {bet}");
        await unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid betId)
    {
        logger.LogDebug($"Deleting bet with id {betId}");
        await unitOfWork.BetRepository.DeleteByIdAsync(betId);
        await unitOfWork.SaveChangesAsync();
        logger.LogDebug($"Bet with id {betId} deleted");
    }
}