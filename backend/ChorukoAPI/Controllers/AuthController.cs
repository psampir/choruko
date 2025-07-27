using ChorukoAPI.DTOs.User;
using ChorukoAPI.Services.Abstraction;
using ChorukoAPI.Services.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ChorukoAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService _authService) : ControllerBase
{    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationDto registrationDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var result = await _authService.RegisterUserAsync(registrationDto);

        if (result.Succeeded)
        {
            return Ok(new { Message = "User registered successfully!" });
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginUserAsync(loginDto);

        if (result.Succeeded)
        {
            return Ok(new { Message = "Login successful!" });
        }

        return BadRequest(ModelState);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserInfoDto>> GetUserInfo()
    {
        var userInfo = await _authService.GetUserInfoAsync();
        return Ok(userInfo);
    }
}
