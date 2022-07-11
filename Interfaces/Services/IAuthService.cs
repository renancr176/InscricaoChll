using InscricaoChll.Api.Models.Requests;
using InscricaoChll.Api.Models.Responses;

namespace InscricaoChll.Api.Interfaces.Services;

public interface IAuthService
{
    Task<BaseResponse<SignInResponse>> SignInAsync(SignInRequest request);
}