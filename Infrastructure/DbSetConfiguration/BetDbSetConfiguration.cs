using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.DbSetConfiguration;

public class BetDbSetConfiguration : BaseEntityDbSetConfiguration<Bet>
{
    protected override void AddConfiguration(EntityTypeBuilder<Bet> builder)
    {
        builder.ToTable("Bet");

        builder
            .HasMany(x => x.Names)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
    }
}