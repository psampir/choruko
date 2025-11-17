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
using ChorukoAPI.Extensions;
using ChorukoAPI.Models;
using ChorukoAPI.Mappers;
using Microsoft.AspNetCore.Mvc;
using System.Net; 


namespace ChorukoAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IAuthService _authService,
    UserManager<User> _userManager,
    IEmailService _emailService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<IdentityResult>>> Register([FromBody] UserRegistrationRequest registrationRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RegisterUserAsync(registrationRequest);

        return result.ToCreatedApiResponse("Password");
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<Microsoft.AspNetCore.Identity.SignInResult>>> Login([FromBody] UserLoginRequest loginRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginUserAsync(loginRequest);

        return result.ToCreatedApiResponse("Login");
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> LogOut()
    {
        var result = await _authService.LogoutUserAsync();

        return result.ToApiResponse("Log out");
    }
    
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        
        if (user == null)
        {
            var response = ServiceResponseFactory.Success(
                new OperationResultDto { Operation = "ForgotPassword", Timestamp = DateTime.UtcNow },
                "If an account exists for this email, a password reset link has been sent.",
                HttpStatusCode.OK
            );
            return response.ToApiResponse("ForgotPassword");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        await _emailService.SendPasswordResetEmailAsync(user.Email!, token);

        var successResponse = ServiceResponseFactory.Success(
            new OperationResultDto 
            { 
                Operation = "ForgotPassword", 
                Timestamp = DateTime.UtcNow, 
                Metadata = new Dictionary<string, object> { { "UserId", user.Id } } 
            },
            "If an account exists for this email, a password reset link has been sent.",
            HttpStatusCode.OK
        );
        return successResponse.ToApiResponse("ForgotPassword");

    }
    
    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        
        if (user == null)
        {
            var response = ServiceResponseFactory.Error<OperationResultDto>(
                null, 
                "Invalid email or token provided.", 
                HttpStatusCode.BadRequest
            );
            return response.ToApiResponse("ResetPassword");
        }

        var decodedToken = WebUtility.UrlDecode(request.Token);
        
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

        if (result.Succeeded)
        {
            var successResponse = ServiceResponseFactory.Success(
                new OperationResultDto 
                { 
                    Operation = "ResetPassword", 
                    Timestamp = DateTime.UtcNow, 
                    Metadata = new Dictionary<string, object> { { "UserId", user.Id } } 
                },
                "Password has been successfully reset.",
                HttpStatusCode.OK
            );
            return successResponse.ToApiResponse("ResetPassword");
        }
        
        var errors = result.Errors.Select(e => e.Description);
        var errorMessage = "Password reset failed: " + string.Join(", ", errors);
        
        var failureResponse = ServiceResponseFactory.Error<OperationResultDto>(
            null, // Result is null on failure
            errorMessage,
            HttpStatusCode.BadRequest 
        );
        
        return failureResponse.ToApiResponse("NewPassword"); 
    }
}
