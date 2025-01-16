using System.Reflection;
using Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DAL;

public class BetDbContext : IdentityDbContext<User, Role, long, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
{
    public BetDbContext(DbContextOptions<BetDbContext> options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        modelBuilder.Entity<BetGame>().HasData(
            new BetGame()
            {
                Id = Guid.Parse("F570D572-7098-464C-8A2F-8CC3ED486C0A"), 
                Name = "Initial",
            });

        modelBuilder.Entity<Result>().HasData(
            new Result
            {
                Id = Guid.Parse("600C3DD3-2045-4FF7-B0EE-7DB638A038EC"),
                BetGameId = Guid.Parse("F570D572-7098-464C-8A2F-8CC3ED486C0A")
            });
    }
}