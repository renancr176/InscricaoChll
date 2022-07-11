using System.Net;
using InscricaoChll.Api.Interfaces.Services;
using InscricaoChll.Api.Models.Requests;
using InscricaoChll.Api.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace InscricaoChll.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    [HttpPost("SignIn")]
    [SwaggerResponse(200, Type = typeof(BaseResponse<SignInResponse>))]
    [SwaggerResponse(400, Type = typeof(BaseResponse))]
    public async Task<IActionResult> SignInAsync([FromBody] SignInRequest request)
    {
        if (!ModelState.IsValid) return InvalidModelResponse();

        var response = await _authService.SignInAsync(request);

        return Response(response, !response.Success ? HttpStatusCode.Unauthorized : HttpStatusCode.OK);
    }

    [HttpPost("PasswordReset")]
    [SwaggerResponse(200, Type = typeof(BaseResponse))]
    [SwaggerResponse(400, Type = typeof(BaseResponse))]
    public async Task<IActionResult> PasswordResetAsync([FromBody] PasswordResetRequest request)
    {
        if (!ModelState.IsValid) return InvalidModelResponse();

        return Response(await _userService.PasswordResetAsync(request));
    }

    [HttpPost("ResetPassword")]
    [SwaggerResponse(200, Type = typeof(BaseResponse))]
    [SwaggerResponse(400, Type = typeof(BaseResponse))]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(InvalidModelResponse());

        return Response(await _userService.ResetPasswordAsync(request));
    }

}