using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.DbSetConfiguration;

public abstract class BaseEntityDbSetConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
    protected abstract void AddConfiguration(EntityTypeBuilder<T> builder);

    public void Configure(EntityTypeBuilder<T> builder)
    {
        AddConfiguration(builder);
    }
}