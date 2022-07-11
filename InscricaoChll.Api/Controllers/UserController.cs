using InscricaoChll.Api.DbContexts.ChllDbContext.Enums;
using InscricaoChll.Api.Interfaces.Services;
using InscricaoChll.Api.Models;
using InscricaoChll.Api.Models.Requests;
using InscricaoChll.Api.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace InscricaoChll.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize("Bearer", Roles = "Admin")]
#if DEBUG
    [ApiExplorerSettings(IgnoreApi = false)]
#else
    [ApiExplorerSettings(IgnoreApi = true)]
#endif  
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IStringLocalizer<UserController> _localizer;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(IUserService userService, IStringLocalizer<UserController> localizer,
            RoleManager<IdentityRole> roleManager)
        {
            _userService = userService;
            _localizer = localizer;
            _roleManager = roleManager;
        }

        [HttpGet("Search")]
        [SwaggerResponse(200, Type = typeof(BaseResponse<IEnumerable<UserModel>>))]
        [SwaggerResponse(400, Type = typeof(BaseResponse))]
        public async Task<IActionResult> SearchAsync([FromQuery] UserSearchRequest request)
        {
            if (!ModelState.IsValid) return InvalidModelResponse();

            return Response(await _userService.SearchAsync(request));
        }

        [HttpPost]
        [SwaggerResponse(200, Type = typeof(BaseResponse<UserModel>))]
        [SwaggerResponse(400, Type = typeof(BaseResponse))]
        public async Task<IActionResult> CreateAsync([FromBody] UserCreateRequest request)
        {
            if (!ModelState.IsValid) return InvalidModelResponse();
           
            return Response(await _userService.SignUpAsync(request, request.Roles));
        }

        [HttpPost("CreateRole/{roleName}")]
        [SwaggerResponse(200, Type = typeof(BaseResponse))]
        [SwaggerResponse(400, Type = typeof(BaseResponse))]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName)
            || string.IsNullOrWhiteSpace(roleName)
            || roleName.Trim().Contains(" ")
            || roleName.Length < 3)
                return BadRequest(new BaseResponse()
                {
                    Errors = new List<BaseResponseError>()
                    {
                        new BaseResponseError()
                        {
                            ErrorCode = "InvalidRoleName",
                            Message = _localizer.GetString("Error_InvalidRoleName")
                        }
                    }
                });

            if (await _roleManager.RoleExistsAsync(roleName))
                return BadRequest(new BaseResponse()
                {
                    Errors = new List<BaseResponseError>()
                    {
                        new BaseResponseError()
                        {
                            ErrorCode = "RoleAlreadyExists",
                            Message = _localizer.GetString("Error_RoleAlreadyExists")
                        }
                    }
                });

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                return BadRequest(new BaseResponse()
                {
                    Errors = result.Errors.Select(e => new BaseResponseError()
                    {
                        ErrorCode = e.Code,
                        Message = e.Description
                    }).ToList()
                });
            }

            return Ok(new BaseResponse());
        }

        [HttpDelete("DeleteRole/{id}")]
        [SwaggerResponse(200, Type = typeof(BaseResponse))]
        [SwaggerResponse(400, Type = typeof(BaseResponse))]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
                return BadRequest(new BaseResponse()
                {
                    Errors = new List<BaseResponseError>()
                    {
                        new BaseResponseError()
                        {
                            ErrorCode = "RoleNotExists.",
                            Message = _localizer.GetString("Error_RoleNotExists")
                        }
                    }
                });

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
            {
                return BadRequest(new BaseResponse()
                {
                    Errors = result.Errors.Select(e => new BaseResponseError()
                    {
                        ErrorCode = e.Code,
                        Message = e.Description
                    }).ToList()
                });
            }

            return Ok(new BaseResponse());
        }

        [HttpPost("AddRole")]
        [SwaggerResponse(200, Type = typeof(BaseResponse))]
        [SwaggerResponse(400, Type = typeof(BaseResponse))]
        public async Task<IActionResult> AddRole([FromBody] UserAddRoleRequest request)
        {
            var userResponse = await _userService.FindByUserName(request.UserName);

            if (!userResponse.Success)
                return Response(userResponse);

            return Response(await _userService.AddRoles(userResponse.Data.Id, request.Roles));
        }

        [HttpPatch("Status")]
        [SwaggerResponse(200, Type = typeof(BaseResponse))]
        [SwaggerResponse(400, Type = typeof(BaseResponse))]
        public async Task<IActionResult> ChangeStatus([FromBody] UserChangeStatusRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(InvalidModelResponse());

            return Response(await _userService.ChangeStatus(request));
        }

        [HttpPost("ConfirmEmail")]
        [SwaggerResponse(200, Type = typeof(BaseResponse))]
        [SwaggerResponse(400, Type = typeof(BaseResponse))]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(InvalidModelResponse());

            return Response(await _userService.ConfirmEmailAsync(request));
        }

    }
}
