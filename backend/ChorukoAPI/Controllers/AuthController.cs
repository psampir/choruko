using ChorukoAPI.Contracts.DTOs;
using ChorukoAPI.Contracts.DTOs.User;
using ChorukoAPI.Contracts.Requests.User;
using ChorukoAPI.Mappers;
using ChorukoAPI.Services.Abstraction;
using ChorukoAPI.Services.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserInfoDto>>> GetUserInfo()
    {
        var result = await _authService.GetUserInfoAsync();

        return result.ToApiResponse();
    }
}
