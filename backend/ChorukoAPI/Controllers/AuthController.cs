using ChorukoAPI.DTOs.User;
using ChorukoAPI.Services.Implementation;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ChorukoAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }
    
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
}
