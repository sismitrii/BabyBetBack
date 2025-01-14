using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Extension;

public static class QueryableExtension
{
    public static IQueryable<Bet> IncludeAll(this IQueryable<Bet> query)
    {
        return query
                .Include(b => b.User)
                .Include(b => b.Names);
    }
    
    public static IQueryable<Bet> IncludeNames(this IQueryable<Bet> query)
    {
        return query
            .Include(b => b.Names);
    }
    
    public static IQueryable<BetGame> IncludeBetData(this IQueryable<BetGame> query)
    {
        return query
            .Include(b => b.Bets)
            .ThenInclude(b => b.Names);
    }
}