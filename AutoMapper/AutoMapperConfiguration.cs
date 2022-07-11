using AutoMapper;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

namespace InscricaoChll.Api.AutoMapper;

public static class AutoMapperConfiguration
{
    public static void AddAutoMapperProfiles(this IServiceCollection services)
    {
        services.AddSingleton(RegisterMappings().CreateMapper());
        services.AddSingleton<IMapper>(sp =>
            new Mapper(sp.GetRequiredService<IConfigurationProvider>(), sp.GetService));
    }

    public static MapperConfiguration RegisterMappings()
    {
        return new MapperConfiguration(ps =>
        {
            ps.AddProfile(new EntityToModelMappingProfile());
            ps.AddProfile(new ModelToEntityMappingProfile());
        });
    }
}