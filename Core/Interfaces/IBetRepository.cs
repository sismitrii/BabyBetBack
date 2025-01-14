using Core.Entities;

namespace Core.Interfaces;

public interface IBetRepository : IBaseRepository<Bet>
{
    Task<bool> ExistBetForUserAsync(User user);
    Task<Bet?> FindByUserIdentifier(string email);
}