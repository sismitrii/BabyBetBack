using Core.Interfaces;

namespace DAL.Repositories;

public class UnitOfWork(BetDbContext context) : IUnitOfWork
{
    private BetDbContext _context { get; } = context;

    public IBetRepository BetRepository { get; } = new BetRepository(context);
    public IBetGameRepository BetGameRepository { get; } = new BetGameRepository(context);
    public IUserRepository UserRepository { get; } = new UserRepository(context);
    
    public Task SaveChangesAsync() => _context.SaveChangesAsync();
    
    public void Dispose()
    {
        _context.Dispose();
    }
}