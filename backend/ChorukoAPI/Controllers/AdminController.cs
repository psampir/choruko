using Microsoft.AspNetCore.Mvc;
using ChorukoAPI.Contracts.Requests.User;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.User;
using ChorukoAPI.Mappers;
using ChorukoAPI.Services.Abstraction;

namespace ChorukoAPI.Controllers
{
    [ApiController]
    [Route("api/Admin")]
    public class AdminController(IAdminUserService adminUserService) : ControllerBase
    {
        [HttpPost("users/reset-password")]
        public async Task<ActionResult<ApiResponse<AdminResetPasswordResponse>>> ResetUserPassword([FromBody] AdminResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await adminUserService.ResetUserPasswordAsync(request);
            return result.ToApiResponse("Reset password");
        }
    }
}
