using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public abstract class BaseRepository<T>(BetDbContext context) : IBaseRepository<T>
    where T : BaseEntity
{
    protected readonly DbSet<T> _dbSet = context.Set<T>();
    
    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public void Delete(T entity) => _dbSet.Remove(entity);

    public async Task DeleteByIdAsync(Guid betId)
    {
        var entity = await _dbSet.FindAsync(betId);
        if (entity != null)
            _dbSet.Remove(entity);
    } 

    public virtual async Task<T?> FindByIdAsync(Guid id) => await _dbSet.FirstOrDefaultAsync(e => e.Id == id);

    public virtual async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
}