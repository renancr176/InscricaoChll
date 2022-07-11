using InscricaoChll.Api.Interfaces.Services;

namespace InscricaoChll.Api.Services;

public static class Services
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITemplateService, TemplateService>();
    }
}