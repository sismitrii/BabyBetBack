using Core.Entities;

namespace Core.Interfaces;

public interface IUserRepository
{
    Task<User?> FindUserByEmailAsync(string? email);
}