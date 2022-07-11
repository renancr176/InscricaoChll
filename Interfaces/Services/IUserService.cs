using InscricaoChll.Api.DbContexts.ChllDbContext.Entities;
using InscricaoChll.Api.DbContexts.ChllDbContext.Enums;
using InscricaoChll.Api.Models;
using InscricaoChll.Api.Models.Requests;
using InscricaoChll.Api.Models.Responses;

namespace InscricaoChll.Api.Interfaces.Services;

public interface IUserService
{
    Task<UserEntity> CurrentUserAsync();
    Task<BaseResponse<UserModel>> SignUpAsync(SignUpRequest request, IEnumerable<RoleEnum> roles = null);
    Task<BaseResponse<object>> PasswordResetAsync(PasswordResetRequest request);
    Task<BaseResponse<object>> ResetPasswordAsync(ResetPasswordRequest request);
    Task<BaseResponse> AddRoles(Guid userId, IEnumerable<RoleEnum> roles);
    Task<BaseResponse<UserModel>> FindByUserName(string userName);
    Task<bool> HasRole(string roleName);
    Task<bool> HasRole(RoleEnum role);
    Task<BaseResponse> ChangeStatus(UserChangeStatusRequest request);
    Task<PagedResponse<UserModel>> SearchAsync(UserSearchRequest request);
    Task<BaseResponse> SendEmailConfirmationAsync(Guid userId);
    Task<BaseResponse> ConfirmEmailAsync(ConfirmEmailRequest request);
}