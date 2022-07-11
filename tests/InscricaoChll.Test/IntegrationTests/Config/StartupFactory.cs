using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace InscricaoChll.Test.IntegrationTests.Config;

public class StartupFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    public string Environment { get; private set; }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseStartup<TStartup>();
        Environment = "Testing";
        builder.UseEnvironment(Environment);
    }
}