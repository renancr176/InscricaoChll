namespace InscricaoChll.Api.Options;

public static class Options
{
    public static void AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GeneralOptions>(configuration.GetSection(GeneralOptions.sectionKey));
    }
}