using InscricaoChll.Api.DbContexts.ChllDbContext.Entities;
using InscricaoChll.Api.DbContexts.ChllDbContext.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InscricaoChll.Api.DbContexts.ChllDbContext.Mappings;

public class UserEntityMap : IEntityTypeConfiguration<UserEntity>
{
    public virtual void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.Property(entity => entity.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(entity => entity.Token)
            .HasMaxLength(255);

        builder.Property(entity => entity.Status)
            .IsRequired()
            .HasDefaultValue(UserStatusEnum.Active);
    }
}