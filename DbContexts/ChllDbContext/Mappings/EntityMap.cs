using InscricaoChll.Api.DbContexts.ChllDbContext.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InscricaoChll.Api.DbContexts.ChllDbContext.Mappings;

public abstract class EntityMap<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : Entity<TEntity>
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.CreatedAt)
            .IsRequired();
    }
}

public abstract class EntityIntIdMap<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : EntityIntId<TEntity>
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .UseMySqlIdentityColumn();

        builder.Property(entity => entity.CreatedAt)
            .IsRequired();
    }
}