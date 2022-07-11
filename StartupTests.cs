using InscricaoChll.Api.AutoMapper;
using InscricaoChll.Api.DbContexts.ChllDbContext;
using InscricaoChll.Api.Options;
using InscricaoChll.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InscricaoChll.Api;

public class StartupTests
{
    public StartupTests()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile($"appsettings.Testing.json", true, true)
            .AddEnvironmentVariables();

        Configuration = builder.Build();
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddAutoMapper(typeof(Startup));
        services.AddAutoMapperProfiles();

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        #region DbContexts

        services.AddDbContext<ChllDbContext>(dbContextOptions =>
            dbContextOptions.UseMySql(Configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(Configuration.GetConnectionString("DefaultConnection"))));

        services.AddChllDbServices();

        #endregion

        #region Options

        var appSettingJwtTokenOptionsSection = Configuration.GetSection(nameof(JwtTokenOptions));
        services.Configure<JwtTokenOptions>(appSettingJwtTokenOptionsSection);

        services.AddOptions(Configuration);

        #endregion

        services.AddServices();

        Init(services.BuildServiceProvider());
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseDeveloperExceptionPage();

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    private void Init(IServiceProvider serviceProvider)
    {
        var dbName = Configuration.GetSection("ConnectionStrings:DefaultConnection").Value
            .Split(";")
            .FirstOrDefault(i => i.Contains("database="))
            .Split("=")
            .LastOrDefault();

        var n3wDbContext = serviceProvider.GetService<ChllDbContext>();

        n3wDbContext.Database.ExecuteSqlRaw($@"DROP DATABASE `{dbName}`;
CREATE DATABASE `{dbName}`;");

        serviceProvider.ChllDbMigrate();
    }
}