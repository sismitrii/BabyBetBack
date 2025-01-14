using Core.Entities;
using Core.Interfaces;
using DAL.Extension;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class BetGameRepository(BetDbContext context) : BaseRepository<BetGame>(context), IBetGameRepository
{
    public override async Task<BetGame?> FindByIdAsync(Guid id) => 
        await _dbSet.IncludeBetData()
            .FirstOrDefaultAsync(e => e.Id == id);
}