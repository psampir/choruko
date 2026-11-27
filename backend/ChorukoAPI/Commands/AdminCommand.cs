using ChorukoAPI.Services.Abstraction;

namespace ChorukoAPI.Commands;

public static class AdminCommand
{
    public static async Task<int> RunAsync(IServiceProvider services, string[] args)
    {
        if (args.Length == 0 || args[0] != "reset-password")
        {
            Console.Error.WriteLine("Usage: choruko-admin reset-password [email]");
            return 1;
        }

        string? email;
        if (args.Length > 1)
        {
            email = args[1];
        }
        else
        {
            Console.Write("Enter the email of the user to reset password: ");
            email = Console.ReadLine();
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            Console.Error.WriteLine("An email address is required.");
            return 1;
        }

        var resetService = services.GetRequiredService<IUserPasswordResetService>();
        var result = await resetService.ResetPasswordAsync(email.Trim());

        if (!result.IsSuccessful || result.Result is null)
        {
            Console.Error.WriteLine(result.Message);
            return 1;
        }

        Console.WriteLine("The password has been reset successfully!");
        Console.WriteLine($"Email        : {result.Result.User.Email}");
        Console.WriteLine($"User ID      : {result.Result.User.Id}");
        Console.WriteLine($"New password : {result.Result.NewPassword}");
        Console.WriteLine("Share the new password with the user securely.");
        return 0;
    }
}
