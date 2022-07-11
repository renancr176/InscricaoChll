using System.Net;
using InscricaoChll.Api.DbContexts.ChllDbContext.Entities;
using InscricaoChll.Api.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace InscricaoChll.Api.Controllers;

public abstract class BaseController : Controller
{
    private readonly IStringLocalizer<Controller> _localizer;

    public BaseController()
    {
    }

    public BaseController(IStringLocalizer<Controller> localizer)
    {
        _localizer = localizer;
    }

    public UserEntity CurrentUser => (UserEntity)HttpContext.User.Identity;

    protected IActionResult InvalidModelResponse()
    {
        return Response(new BaseResponse()
        {
            Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => new BaseResponseError()
            {
                ErrorCode = "ModelError",
                Message = _localizer?.GetString(e.ErrorMessage) ?? e.ErrorMessage
            }).ToList()
        });
    }

    protected new IActionResult Response(BaseResponse response, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        if (!response.Success)
        {
            if (statusCode != HttpStatusCode.OK)
            {
                return StatusCode((int)statusCode, response);
            }

            return BadRequest(response);
        }

        if (statusCode != HttpStatusCode.OK)
        {
            return StatusCode((int)statusCode, response);
        }

        return (response.GetType() != typeof(BaseResponse<object>) || ((BaseResponse<object>)response)?.Data != null)
            ? Ok(response)
            : StatusCode((int)HttpStatusCode.NoContent, response);
    }
}