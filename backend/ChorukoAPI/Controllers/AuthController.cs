using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.User;
using ChorukoAPI.Contracts.Requests.User;
using ChorukoAPI.Mappers;
using ChorukoAPI.Services.Abstraction;
using ChorukoAPI.Services.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ChorukoAPI.Contracts;

namespace ChorukoAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService _authService) : ControllerBase
{    
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<IdentityResult>>> Register([FromBody] UserRegistrationRequest registrationRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RegisterUserAsync(registrationRequest);

        return result.ToCreatedApiResponse();
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<Microsoft.AspNetCore.Identity.SignInResult>>> Login([FromBody] UserLoginRequest loginRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginUserAsync(loginRequest);

        return result.ToCreatedApiResponse();
    }

    [HttpPost("logOut")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> LogOut()
    {
        var result = await _authService.LogoutUserAsync();

        return result.ToApiResponse();
    }

    [HttpGet("userInfo")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserInfoResponse>>> GetUserInfo()
    {
        var result = await _authService.GetUserInfoAsync();

        return result.ToApiResponse();
    }
}
