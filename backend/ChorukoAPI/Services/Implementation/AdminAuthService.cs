using System.Net;
using System.Security.Cryptography;
using System.Text;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Extensions;
using ChorukoAPI.Services.Abstraction;

namespace ChorukoAPI.Services.Implementation;

public class AdminAuthService(IConfiguration configuration) : IAdminAuthService
{
    public ServiceResponse<T> VerifySuperuserCredentials<T>(string providedUsername, string providedPassword)
    {
        var dbPassword = configuration["DB_PASSWORD"] ?? Environment.GetEnvironmentVariable("DB_PASSWORD");
        if (string.IsNullOrEmpty(dbPassword))
        {
            return ServiceResponseFactory.Error<T>(
                default!,
                "DB_PASSWORD is not configured.",
                HttpStatusCode.InternalServerError
            );
        }

        var dbUsername = configuration["DB_USERNAME"] ?? Environment.GetEnvironmentVariable("DB_USERNAME");
        if (string.IsNullOrEmpty(dbUsername))
        {
            return ServiceResponseFactory.Error<T>(
                default!,
                "DB_USERNAME is not configured.",
                HttpStatusCode.InternalServerError
            );
        }

        if (!SecureEquals(providedPassword, dbPassword) || !SecureEquals(providedUsername, dbUsername))
        {
            return ServiceResponseFactory.Error<T>(
                default!,
                "Invalid superuser credentials.",
                HttpStatusCode.Forbidden
            );
        }

        return ServiceResponseFactory.Success(
            default(T)!,
            "Credentials verified."
        );
    }

    // Compares two strings in a way that mitigates timing attacks
    private static bool SecureEquals(string provided, string expected)
    {
        var a = SHA256.HashData(Encoding.UTF8.GetBytes(provided));
        var b = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
        return CryptographicOperations.FixedTimeEquals(a, b);
    }
}
