using Application.Dtos.Out.Stats;

namespace Application.Services;

public interface IStatsService
{
    Task<StatsDto?> GetStatsAsync(Guid betGameId);
}