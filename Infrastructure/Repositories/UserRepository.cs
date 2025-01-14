using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class UserRepository(BetDbContext context) : IUserRepository
{
    private readonly DbSet<User> _dbSet = context.Set<User>();

    public async Task<User?> FindUserByEmailAsync(string? email) 
        => await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
}