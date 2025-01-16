using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.DbSetConfiguration;

public class ResultDbSetConfiguration : BaseEntityDbSetConfiguration<Result>
{
    protected override void AddConfiguration(EntityTypeBuilder<Result> builder)
    {
        builder.ToTable("Result");
    }
}