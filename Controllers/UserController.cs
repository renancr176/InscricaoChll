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

        public UserController(IUserService userService)
        {
            _userService = userService;
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
        [AllowAnonymous]
        public async Task<IActionResult> SignUpAsync([FromBody] SignUpRequest request)
        {
            if (!ModelState.IsValid) return InvalidModelResponse();
           
            return Response(await _userService.SignUpAsync(request, new [] {RoleEnum.User}));
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
