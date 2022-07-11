using InscricaoChll.Api.DbContexts.ChllDbContext;
using Microsoft.EntityFrameworkCore;

namespace InscricaoChll.Api;

public class StartupTests : IStartup
{
    public StartupTests(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;
    }

    public IConfiguration Configuration { get; }
    public IWebHostEnvironment Environment { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        Init(services.BuildServiceProvider());
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();
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