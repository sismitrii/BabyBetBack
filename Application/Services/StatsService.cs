using Application.Dtos.Out.Stats;
using Core.Entities;
using Core.Enum;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class StatsService(IUnitOfWork unitOfWork, ILogger<StatsService> logger) : IStatsService
{
    public async Task<StatsDto?> GetStatsAsync(Guid betGameId)
    {
        logger.LogDebug($"Getting stats for bet game {betGameId}");
        
        var betGame = await unitOfWork.BetGameRepository.FindByIdAsync(betGameId) ??
                      throw new Exception($"No BetGame found with this betGameId : ${betGameId}");
        
        var bets = betGame.Bets;

        if (bets.Count == 0)
            return null;
        
        return GenerateStats(bets);
    }

    private StatsDto GenerateStats(ICollection<Bet> bets)
    {
        var genderStats = GenerateGenderStats(bets);
        var birthDateStats = GenerateBirthDateStats(bets);
        var sizeStats = GenerateSizeStats(bets);
        var weightStats = GenerateWeightStats(bets);
        var nameStatsDto = GenerateNameStats(bets);

        return new StatsDto
        {
            GenderStats = genderStats,
            BirthDateStats = birthDateStats,
            SizeStats = sizeStats,
            WeightStats = weightStats,
            NamesStats = nameStatsDto
        };
    }

    private GenderStatsDto GenerateGenderStats(ICollection<Bet> bets)
    {
        var boyCount = bets.Count(x => x.Gender == Gender.Boy) * 1.0;
        var girlCount = bets.Count(bet => bet.Gender == Gender.Girl)* 1.0;
        var boyPercentage = boyCount/bets.Count*100.0;
        var girlPercentage = girlCount/bets.Count*100.0;;

        return new GenderStatsDto
        {
            BoyPercentage = boyPercentage,
            GirlPercentage = girlPercentage,
        };
    }
    
    private IEnumerable<BirthDateStatsDto> GenerateBirthDateStats(ICollection<Bet> bets)
    {
        return bets.GroupBy(x => x.BirthDate)
            .Select(g => new BirthDateStatsDto()
            {
                Date = g.Key,
                Count = g.Count()
            })
            .ToList();
    }
    
    private IEnumerable<SizeStatsDto> GenerateSizeStats(ICollection<Bet> bets)
    {
        return bets.GroupBy(x => x.Size)
            .Select(g => new SizeStatsDto()
            {
                Size = g.Key,
                Count = g.Count(),
                Percentage = Math.Round(g.Count() *1.0 /bets.Count *1.0 * 100.0)
            })
            .OrderBy(x => x.Size)
            .ToList();
    }
    
    private IEnumerable<WeightStatsDto> GenerateWeightStats(ICollection<Bet> bets)
    {
        return bets.GroupBy(x => x.Weight)
            .Select(g => new WeightStatsDto
            {
                Weight = g.Key,
                Count = g.Count(),
                Percentage = Math.Round(g.Count() *1.0 /bets.Count *1.0 * 100.0)
            })
            .OrderBy(x => x.Weight)
            .ToList();    
    }
    
    private IEnumerable<NameStatsDto> GenerateNameStats(ICollection<Bet> bets)
    {
        return bets.SelectMany(x => x.Names)
            .GroupBy(x => x.Value.ToLower())
            .Select(g => new NameStatsDto
            {
                Name = g.Key[0].ToString().ToUpper() + g.Key[1..], // TODO : Creer methode static pour mettre en capital la 1ere lettre 
                Count = g.Count()
            })
            .ToList();
        
    }
}