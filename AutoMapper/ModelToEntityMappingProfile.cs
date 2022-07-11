using AutoMapper;
using InscricaoChll.Api.DbContexts.ChllDbContext.Entities;
using InscricaoChll.Api.Models.Requests;

namespace InscricaoChll.Api.AutoMapper;

public class ModelToEntityMappingProfile : Profile
{
    public ModelToEntityMappingProfile()
    {
        CreateMap<SignUpRequest, UserEntity>();
    }
}