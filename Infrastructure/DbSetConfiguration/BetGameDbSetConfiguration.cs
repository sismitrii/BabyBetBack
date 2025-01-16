using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.DbSetConfiguration;

public class BetGameDbSetConfiguration : BaseEntityDbSetConfiguration<BetGame>
{
    protected override void AddConfiguration(EntityTypeBuilder<BetGame> builder)
    {
        builder.ToTable("BetGame");

        builder
            .HasMany(x => x.Bets)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Result)
            .WithOne()
            .HasForeignKey<Result>(x => x.BetGameId)
            .OnDelete(DeleteBehavior.Cascade);
        
    }
}