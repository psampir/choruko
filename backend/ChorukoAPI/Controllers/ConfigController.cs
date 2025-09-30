using Microsoft.AspNetCore.Mvc;

namespace ChorukoAPI.Controllers;

[ApiController]
[Route("api/config")]
public class ConfigController(IConfiguration config) : ControllerBase
{
    [HttpGet("email-enabled")]
    public IActionResult GetEmailEnabled()
    {
        var raw = config["ENABLE_EMAILS"];
        var enabled = ParseBool(raw);
        return Ok(new { enableEmails = enabled });
    }

    private static bool ParseBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (bool.TryParse(value, out var b)) return b;

        return value.Trim().ToLowerInvariant() switch
        {
            "1" or "yes" or "y" or "on" => true,
            _ => false
        };
    }
}