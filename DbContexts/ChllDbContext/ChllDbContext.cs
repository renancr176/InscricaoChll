using InscricaoChll.Api.DbContexts.ChllDbContext.Entities;
using InscricaoChll.Api.DbContexts.ChllDbContext.Mappings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InscricaoChll.Api.DbContexts.ChllDbContext;

public class ChllDbContext : IdentityDbContext<UserEntity, IdentityRole<Guid>, Guid>
{
    public ChllDbContext()
    {
    }

    public ChllDbContext(DbContextOptions<ChllDbContext> options)
    : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            optionsBuilder.UseMySql(config.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(config.GetConnectionString("DefaultConnection")));
        }
    }

    #region DbSets


    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        #region Mappings

        modelBuilder.ApplyConfiguration(new UserEntityMap());

        #endregion
    }
}