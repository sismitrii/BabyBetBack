namespace Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IBetRepository BetRepository { get; }
    IUserRepository UserRepository { get; }
    IBetGameRepository BetGameRepository { get; }
    Task SaveChangesAsync();
    
}