using System.Security.Claims;
using AutoMapper;
using InscricaoChll.Api.DbContexts.ChllDbContext;
using InscricaoChll.Api.DbContexts.ChllDbContext.Entities;
using InscricaoChll.Api.DbContexts.ChllDbContext.Enums;
using InscricaoChll.Api.Extensions;
using InscricaoChll.Api.Interfaces.Services;
using InscricaoChll.Api.Models;
using InscricaoChll.Api.Models.Requests;
using InscricaoChll.Api.Models.Responses;
using InscricaoChll.Api.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InscricaoChll.Api.Services;

public class UserService : IUserService
{
    private readonly IOptions<GeneralOptions> _generalOptions;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<UserEntity> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IMapper _mapper;
    private readonly ChllDbContext _chllDbContext;
    private readonly ITemplateService _templateService;

    private string _currentUserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public UserEntity CurrentUser { get; private set; }
    private GeneralOptions GeneralOptions => _generalOptions.Value;

    public UserService(IOptions<GeneralOptions> generalOptions, IHttpContextAccessor httpContextAccessor,
        UserManager<UserEntity> userManager, RoleManager<IdentityRole<Guid>> roleManager, IMapper mapper,
        ChllDbContext chllDbContext, ITemplateService templateService)
    {
        _generalOptions = generalOptions;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
        _chllDbContext = chllDbContext;
        _templateService = templateService;
    }

    public async Task<UserEntity> CurrentUserAsync()
    {
        if (CurrentUser == null
            && !string.IsNullOrEmpty(_currentUserId))
        {
            CurrentUser = await _userManager.FindByIdAsync(_currentUserId);
        }

        return CurrentUser;
    }

    public async Task<BaseResponse<UserModel>> SignUpAsync(SignUpRequest request, IEnumerable<RoleEnum> roles = null)
    {
        var response = new BaseResponse<UserModel>();

        try
        {
            var user = _mapper.Map<UserEntity>(request);

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                response.Errors = result.Errors.Select(e => new BaseResponseError()
                {
                    ErrorCode = e.Code,
                    Message = e.Description
                }).ToList();
            }

            if (roles != null)
            {
                await AddRoles(user.Id, roles);
            }

            if (roles == null || !roles.Contains(RoleEnum.User))
            {
                await SendEmailConfirmationAsync(user.Id);
            }

            response.Data = _mapper.Map<UserModel>(user);
            response.Data.Roles = roles;
        }
        catch (Exception e)
        {
            response.Errors.Add(new BaseResponseError()
            {
                ErrorCode = "InternalServerError",
                Message = "Erro interno do servidor, tente novamente mais tarde."
            });
        }

        return response;
    }

    public async Task<BaseResponse<object>> PasswordResetAsync(PasswordResetRequest request)
    {
        var response = new BaseResponse<object>();

        try
        {
            var user = (UserEntity)await _userManager.FindByNameAsync(request.UserName);

            if (user is null)
            {
                response.Errors.Add(new BaseResponseError()
                {
                    ErrorCode = "UserNotFound",
                    Message = "Usuário não encontrado."
                });

                return response;
            }

            user.Token = await _userManager.GeneratePasswordResetTokenAsync(user);
            user.TokenExpiration = DateTime.UtcNow.AddMinutes(5);
            await _userManager.UpdateAsync(user);

            var body = await _templateService.GetContent("EmailPasswordReset.html",
                new Dictionary<string, string>()
                {
                    {"#Name", user.Name},
                    {"#Url", GeneralOptions.FrontUrl},
                    {"#Token", user.Token.Base64Encode()}
                });
                
            /*if (!await _mailService.SendAsync(new SendMailResquest(user.Email, _localizer.GetString("Subject_PasswordReset"), body)))
            {
                response.Errors.Add(new BaseResponseError()
                {
                    ErrorCode = "UnableSendMail",
                    Message = _localizer.GetString("Error_UnableSendMail")
                });

                return response;
            }*/

            response.Data = new
            {
                Message = "Verifique seu e-mail para obter mais instruções."
            };
        }
        catch (Exception e)
        {
            response.Errors.Add(new BaseResponseError()
            {
                ErrorCode = "InternalServerError",
                Message = "Erro interno do servidor, tente novamente mais tarde."
            });
        }

        return response;
    }

    public async Task<BaseResponse<object>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var response = new BaseResponse<object>();

        try
        {
            var token = request.Token.IsBase64Encoded() ? request.Token.Base64Decode() : request.Token;
            var user = _chllDbContext.Users.FirstOrDefault(u =>
                u.Token == token);

            if (user == null
            || !await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider,
                    "ResetPassword", token))
            {
                response.Errors.Add(new BaseResponseError()
                {
                    ErrorCode = "InvalidToken",
                    Message = "O tempo de alteração de senha expirou ou a chave é inválida."
                });

                return response;
            }

            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (!result.Succeeded)
            {
                response.Errors = result.Errors.Select(e => new BaseResponseError()
                {
                    ErrorCode = e.Code,
                    Message = e.Description
                }).ToList();
                return response;
            }

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            user.Token = null;
            user.TokenExpiration = null;
            await _userManager.UpdateAsync(user);

            response.Data = new
            {
                Message = "Senha alterada com sucesso."
            };
        }
        catch (Exception e)
        {
            response.Errors.Add(new BaseResponseError()
            {
                ErrorCode = "InternalServerError",
                Message = "Erro interno do servidor, tente novamente mais tarde."
            });
        }

        return response;
    }

    public async Task<BaseResponse> AddRoles(Guid userId, IEnumerable<RoleEnum> roles)
    {
        var response = new BaseResponse();

        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                response.Errors.Add(new BaseResponseError()
                {
                    ErrorCode = "UserNotFound",
                    Message = "Usuário não encontrado."
                });
                return response;
            }

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role.ToString()))
                {
                    response.Errors.Add(new BaseResponseError()
                    {
                        ErrorCode = "RoleNotFound",
                        Message = "Permissão não encontrada."
                            .ToString()
                            .Replace("#Role", role.ToString())
                    });

                    return response;
                }
            }

            foreach (var role in roles)
            {
                await _userManager.AddToRoleAsync(user, role.ToString());
            }
        }
        catch (Exception e)
        {
            response.Errors.Add(new BaseResponseError()
            {
                ErrorCode = "InternalServerError",
                Message = "Erro interno do servidor, tente novamente mais tarde."
            });
        }

        return response;
    }

    public async Task<BaseResponse<UserModel>> FindByUserName(string userName)
    {
        var response = new BaseResponse<UserModel>();

        try
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                response.Errors.Add(new BaseResponseError()
                {
                    ErrorCode = "UserNotFound",
                    Message = "Usuário não encontrado."
                });
            }

            response.Data = _mapper.Map<UserModel>(user);
        }
        catch (Exception e)
        {
            response.Errors.Add(new BaseResponseError()
            {
                ErrorCode = "InternalServerError",
                Message = "Erro interno do servidor, tente novamente mais tarde."
            });
        }

        return response;
    }

    public async Task<bool> HasRole(string roleName)
    {
        var user = await CurrentUserAsync();
        if (user == null)
            return false;

        return _httpContextAccessor.HttpContext?.User.IsInRole(roleName) ?? false;
    }

    public async Task<bool> HasRole(RoleEnum role)
    {
        return await HasRole(role.ToString());
    }

    public async Task<BaseResponse> ChangeStatus(UserChangeStatusRequest request)
    {
        var response = new BaseResponse();

        try
        {
            var currentUser = await CurrentUserAsync();
            if (currentUser != null && currentUser.Id == request.UserId)
            {
                response.Errors.Add(new BaseResponseError()
                {
                    ErrorCode = "UserCannotChangeOwnStatus",
                    Message = "Você não pode alterar o status da sua própria conta."
                });
                return response;
            }

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user == null)
            {
                response.Errors.Add(new BaseResponseError()
                {
                    ErrorCode = "UserNotFound",
                    Message = "Usuário não encontrado."
                });
            }

            user.Status = request.Status;

            await _userManager.UpdateAsync(user);
        }
        catch (Exception e)
        {
            response.Errors.Add(new BaseResponseError()
            {
                ErrorCode = "InternalServerError",
                Message = "Erro interno do servidor, tente novamente mais tarde."
            });
        }

        return response;
    }

    public async Task<PagedResponse<UserModel>> SearchAsync(UserSearchRequest request)
    {
        var response = new PagedResponse<UserModel>();

        try
        {
            var userModels = _chllDbContext.Users
                .Join(_chllDbContext.UserRoles,
                    x => x.Id,
                    y => y.UserId,
                    (x, y) => new { user = x, userRole = y })
                .Join(_chllDbContext.Roles,
                    x => x.userRole.RoleId,
                    y => y.Id,
                    (x, y) => new { x.user, role = y.Name })
                .Where(x => (request.HasRoles == null || !request.HasRoles.Any() || request.HasRoles.Select(re => re.ToString()).Contains(x.role))
                    && (request.NotHaveRoles == null || !request.NotHaveRoles.Any() || !request.NotHaveRoles.Select(re => re.ToString()).Contains(x.role))
                    && (string.IsNullOrEmpty(request.Name) || x.user.Name.Contains(request.Name))).AsEnumerable()
                .GroupBy(x => x.user)
                .Select(x => new UserModel
                {
                    Id = x.Key.Id,
                    Name = x.Key.Name,
                    UserName = x.Key.UserName,
                    Email = x.Key.Email,
                    Status = x.Key.Status,
                    Roles = Enum.GetValues<RoleEnum>().Where(role => x.Select(x => x.role).ToList().Contains(role.ToString()))
                })
                .ToList();

            response.TotalCount = userModels.Count();
            response.TotalPages = (int)Math.Ceiling(response.TotalCount / (double)request.PageSize);
            response.Data = userModels.Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize).AsEnumerable();
            return response;

        }
        catch (Exception e)
        {
            response.Errors.Add(new BaseResponseError
            {
                ErrorCode = "UnableToGetUsers",
                Message = "Não foi possível obter os usuário."
            });
        }

        return response;
    }

    public async Task<BaseResponse> SendEmailConfirmationAsync(Guid userId)
    {
        var response = new BaseResponse();
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null || user.EmailConfirmed)
            {
                return response;
            }

            user.Token = string.Empty.GenerateTokenString(6);
            user.TokenExpiration = DateTime.UtcNow.AddMinutes(5);
            await _userManager.UpdateAsync(user);

            var body = await _templateService.GetContent("EmailConfirmation.html",
                new Dictionary<string, string>()
                {
                    {"#Name", user.Name},
                    {"#Url", GeneralOptions.FrontUrl},
                    {"#Token", user.Token.Base64Encode()}
                });

            /*if (!await _mailService.SendAsync(new SendMailResquest(user.Email, _localizer.GetString("Subject_EmailConfirmation"), body)))
            {
                response.Errors.Add(new BaseResponseError()
                {
                    ErrorCode = "UnableSendMail",
                    Message = _localizer.GetString("Error_UnableSendMail")
                });

                return response;
            }*/
        }
        catch (Exception e)
        {
            response.Errors.Add(new BaseResponseError()
            {
                ErrorCode = "InternalServerError",
                Message = "Erro interno do servidor, tente novamente mais tarde."
            });
        }

        return response;
    }

    public async Task<BaseResponse> ConfirmEmailAsync(ConfirmEmailRequest request)
    {
        var response = new BaseResponse();

        try
        {
            var user = await _chllDbContext.Users.FirstOrDefaultAsync(u =>
                u.Token == request.Token
                && u.TokenExpiration > DateTime.UtcNow);
            
            if (user == null)
            {
                response.Errors.Add(new BaseResponseError()
                {
                    ErrorCode = "InvalidToken",
                    Message = "O tempo de confirmação de email expirou ou a chave é inválida."
                });

                return response;
            }

            user.EmailConfirmed = true;
            user.Token = null;
            user.TokenExpiration = null;

            await _userManager.UpdateAsync(user);
        }
        catch (Exception e)
        {
            response.Errors.Add(new BaseResponseError()
            {
                ErrorCode = "InternalServerError",
                Message = "Erro interno do servidor, tente novamente mais tarde."
            });
        }

        return response;
    }
}