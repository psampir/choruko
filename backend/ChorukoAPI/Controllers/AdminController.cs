using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ChorukoAPI.Models;
using ChorukoAPI.Contracts.Requests.User;
using ChorukoAPI.Contracts.Responses.User;

namespace ChorukoAPI.Controllers
{
    [ApiController]
    [Route("api/Admin")]
    public class AdminController(UserManager<User> userManager, IConfiguration config) : ControllerBase
    {
        [HttpPost("users/resetPassword")]
        public async Task<ActionResult<AdminResetPasswordResponse>> ResetUserPassword([FromBody] AdminResetPasswordRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1) Verify superuser via DB_USERNAME and DB_PASSWORD env/config
            var dbPassword = config["DB_PASSWORD"] ?? Environment.GetEnvironmentVariable("DB_PASSWORD");
            if (string.IsNullOrEmpty(dbPassword))
                return StatusCode(500, "DB_PASSWORD is not configured.");

            if (!SecureEquals(request.DbPassword ?? string.Empty, dbPassword))
                return Forbid();
            
            var dbUsername = config["DB_USERNAME"] ?? Environment.GetEnvironmentVariable("DB_USERNAME");
            if (string.IsNullOrEmpty(dbUsername))
                return StatusCode(500, "DB_USERNAME is not configured.");
            
            if (!SecureEquals(request.DbUsername ?? string.Empty, dbUsername))
                return Forbid();

            // 2) Find the target user
            var user = await userManager.FindByEmailAsync(request.UserEmail);
            if (user == null) return NotFound("User not found.");

            // 3) Generate a password that satisfies Identity PasswordOptions
            var newPassword = GenerateCompliantPassword(userManager.Options.Password);

            // 4) Reset password using Identity
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await userManager.ResetPasswordAsync(user, token, newPassword);
            if (!resetResult.Succeeded)
                return BadRequest(new { errors = resetResult.Errors.Select(e => e.Description).ToArray() });

            // 5) Refresh stamps and persist
            await userManager.UpdateSecurityStampAsync(user);
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return StatusCode(500, new { errors = updateResult.Errors.Select(e => e.Description).ToArray() });

            return Ok(new AdminResetPasswordResponse
            {
                UserId = user.Id,
                UserEmail = user.Email ?? string.Empty,
                UserNewPassword = newPassword
            });
        }

        // Compares two strings in a way that mitigates timing attacks
        private static bool SecureEquals(string provided, string expected)
        {
            var a = SHA256.HashData(Encoding.UTF8.GetBytes(provided));
            var b = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
            return CryptographicOperations.FixedTimeEquals(a, b);
        }

        // Generates a random password that satisfies ASP.NET Identity PasswordOptions
        private static string GenerateCompliantPassword(PasswordOptions opts)
        {
            // Final length: at least 12 and at least the configured RequiredLength (if > 0)
            var length = Math.Max(opts.RequiredLength > 0 ? opts.RequiredLength : 12, 12);

            // ReSharper disable once StringLiteralTypo
            const string upper = "ABCDEFGHIJKLMNPQRSTUVWXYZ";
            // ReSharper disable once StringLiteralTypo
            const string lower = "abcdefghijkmnopqrstuvwxyz";
            const string digit = "1234567890";
            const string special = "!@$?_-.+*";

            // Collect only the sets mandated by the PasswordOptions flags
            var requiredSets = new List<string>();
            if (opts.RequireUppercase) requiredSets.Add(upper);
            if (opts.RequireLowercase) requiredSets.Add(lower);
            if (opts.RequireDigit) requiredSets.Add(digit);
            if (opts.RequireNonAlphanumeric) requiredSets.Add(special);
            
            // If none of the required sets are specified, use all of them
            if (requiredSets.Count == 0) requiredSets.Add(upper + lower + digit + special);
            
            // Seed: pick a random set of characters from the required sets
            var chars = requiredSets
                .Select(set => set[RandomNumberGenerator.GetInt32(set.Length)])
                .ToList();
            
            // Keep adding random characters until:
            // // 1) We reach the desired length.
            // // 2) We have at least RequiredUniqueChars distinct characters (minimum 1).
            while (chars.Count < length || chars.Distinct().Count() < Math.Max(opts.RequiredUniqueChars, 1))
            {
                var set = requiredSets[RandomNumberGenerator.GetInt32(requiredSets.Count)];
                chars.Add(set[RandomNumberGenerator.GetInt32(set.Length)]);
            }

            // Shuffle the characters
            for (var i = chars.Count - 1; i > 0; i--)
            {
                var j = RandomNumberGenerator.GetInt32(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            return new string(chars.ToArray());
        }
    }
}
