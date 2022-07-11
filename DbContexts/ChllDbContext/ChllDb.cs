using InscricaoChll.Api.DbContexts.ChllDbContext.Interfaces.Seeders;
using InscricaoChll.Api.DbContexts.ChllDbContext.Seeders;
using Microsoft.EntityFrameworkCore;

namespace InscricaoChll.Api.DbContexts.ChllDbContext;

public static class ChllDb
{
    public static void AddChllDbServices(this IServiceCollection services)
    {
        #region Repositories

        #endregion

        #region Validators


        #endregion

        #region Seeders

        services.AddScoped<IRoleSeed, RoleSeed>();

        #endregion
    }

    public static void ChllDbMigrate(this IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetService<ChllDbContext>();
        dbContext.Database.Migrate();

        #region Seeders

        Task.Run(async () =>
        {
            await serviceProvider.GetService<IRoleSeed>().SeedAsync();
        }).Wait();

        #endregion
    }
}