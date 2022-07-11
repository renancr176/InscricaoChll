using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using InscricaoChll.Api.DbContexts.ChllDbContext.Entities;
using InscricaoChll.Api.DbContexts.ChllDbContext.Enums;
using InscricaoChll.Api.Interfaces.Services;
using InscricaoChll.Api.Models;
using InscricaoChll.Api.Models.Requests;
using InscricaoChll.Api.Models.Responses;
using InscricaoChll.Api.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace InscricaoChll.Api.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly SignInManager<UserEntity> _signInManager;
    private readonly IOptions<JwtTokenOptions> _jwtTokenOptions;
    private readonly IUserService _userService;

    private static long ToUnixEpochDate(DateTime date)
        => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);

    public AuthService(UserManager<UserEntity> userManager,
        SignInManager<UserEntity> signInManager, IOptions<JwtTokenOptions> jwtTokenOptions,
        IUserService userService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenOptions = jwtTokenOptions;
        _userService = userService;
    }

    public async Task<BaseResponse<SignInResponse>> SignInAsync(SignInRequest request)
    {
        var response = new BaseResponse<SignInResponse>();

        var result = await _signInManager.PasswordSignInAsync(request.UserName, request.Password, false, true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                response.Errors.Add(new BaseResponseError()
                {
                    ErrorCode = "LoginBlocked",
                    Message = "Login bloqueado, tente novamente mais tarde."
                });
            }
            else
            {
                response.Errors.Add(new BaseResponseError()
                {
                    ErrorCode = "InvalidUseramePassword",
                    Message = "Usuário ou senha inválido."
                });
            }

            return response;
        }

        var user = await _userManager.FindByNameAsync(request.UserName);

        if (!user.EmailConfirmed)
        {
            await _userService.SendEmailConfirmationAsync(user.Id);
            response.Errors.Add(new BaseResponseError()
            {
                ErrorCode = $"EmailConfirmedNotConfirmed",
                Message = "Email não confirmado, verifique seu e-mail para a confirmação de email."
            });
            return response;
        }

        if (user.Status != UserStatusEnum.Active)
        {
            response.Errors.Add(new BaseResponseError()
            {
                ErrorCode = $"User{user.Status}",
                Message = "Usuário desativado."
            });
            return response;
        }

        var userClaims = await _userManager.GetClaimsAsync(user);

        userClaims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()));
        userClaims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        userClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, await _jwtTokenOptions.Value.JtiGenerator()));
        userClaims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtTokenOptions.Value.IssuedAt).ToString(), ClaimValueTypes.Integer64));

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            userClaims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, role));
            userClaims.Add(new Claim("roles", role));
        }

        var jwt = new JwtSecurityToken(
            issuer: _jwtTokenOptions.Value.Issuer,
            audience: _jwtTokenOptions.Value.Audience,
            claims: userClaims,
            notBefore: _jwtTokenOptions.Value.NotBefore,
            expires: _jwtTokenOptions.Value.Expiration,
            signingCredentials: _jwtTokenOptions.Value.SigningCredentials);

        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        response.Data = new SignInResponse()
        {
            AccessToken = encodedJwt,
            ExpiresIn = (int)_jwtTokenOptions.Value.ValidFor.TotalSeconds,
            User = new UserModel()
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Name = user.Name,
                Roles = Enum.GetValues<RoleEnum>().Where(role => roles.Contains(role.ToString())),
                Status = user.Status
            }
        };

        return response;
    }
}