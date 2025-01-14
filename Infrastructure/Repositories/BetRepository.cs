using Core.Entities;
using Core.Interfaces;
using DAL.Extension;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class BetRepository(BetDbContext context) : BaseRepository<Bet>(context), IBetRepository
{
    public override async Task<Bet?> FindByIdAsync(Guid id) => 
        await _dbSet.IncludeNames()
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<bool> ExistBetForUserAsync(User user) 
        => await _dbSet.AnyAsync(x => x.User == user);

    public async Task<Bet?> FindByUserIdentifier(string email)
    {
        return await _dbSet
            .IncludeNames()
            .FirstOrDefaultAsync(x => x.User.Email == email);
    }

    public override async Task<IEnumerable<Bet>> GetAllAsync()
    {
        return await _dbSet
                .IncludeAll()
                .ToListAsync();
    }
}