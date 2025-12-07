using ChorukoAPI.Services.Abstraction;

namespace ChorukoAPI.Services.Implementation;

public class NoOpEmailService : IEmailService
{
    public Task SendPasswordResetEmailAsync(string email, string resetToken)
    {
        return Task.CompletedTask;
    }
}
