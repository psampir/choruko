namespace ChorukoAPI.Services.Abstraction;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string resetToken);
}