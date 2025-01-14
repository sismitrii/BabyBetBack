using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.DbSetConfiguration;

public class NameDbSetConfiguration : BaseEntityDbSetConfiguration<Name>
{
    protected override void AddConfiguration(EntityTypeBuilder<Name> builder)
    {
        builder.ToTable("Name");
    }
}