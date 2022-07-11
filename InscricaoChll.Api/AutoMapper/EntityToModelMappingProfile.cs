using AutoMapper;
using FluentValidation.Results;
using InscricaoChll.Api.DbContexts.ChllDbContext.Entities;
using InscricaoChll.Api.Models;
using InscricaoChll.Api.Models.Responses;

namespace InscricaoChll.Api.AutoMapper;

public class EntityToModelMappingProfile : Profile
{
    public EntityToModelMappingProfile()
    {
        CreateMap<ValidationFailure, BaseResponseError>()
            .ForMember(dest => dest.Message,
                act => act.MapFrom(src => src.ErrorMessage));

        CreateMap<UserEntity, UserModel>();
    }
}