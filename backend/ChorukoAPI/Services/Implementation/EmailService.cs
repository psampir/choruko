using ChorukoAPI.Services.Abstraction;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ChorukoAPI.Services.Implementation;

public class EmailService : IEmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPass;
    private readonly string _smtpFrom;
    private readonly string _serverUrl;
    
    public EmailService(
        string smtpHost, 
        int smtpPort, 
        string smtpUser, 
        string smtpPass, 
        string smtpFrom,
        string serverUrl)
    {
        _smtpHost = smtpHost;
        _smtpPort = smtpPort;
        _smtpUser = smtpUser;
        _smtpPass = smtpPass;
        _smtpFrom = smtpFrom;
        _serverUrl = serverUrl;
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetToken)
    {
        try
        {
            using (var client = new SmtpClient(_smtpHost, _smtpPort))
            {
                var resetLink = $"{_serverUrl}/reset-password?email={WebUtility.UrlEncode(email)}&token={WebUtility.UrlEncode(resetToken)}";

                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                
                string htmlBody = $@"
                            <!DOCTYPE html>
                            <html>
                            <head>
                                <meta charset='utf-8'>
                                <meta name='viewport' content='width=device-width, initial-scale=1'>
                                <title>Password Reset</title>
                            </head>
                            <body style='font-family: Asap, sans-serif; background-color: #f1f8e8; margin: 0; padding: 0;'>

                                <table role='presentation' border='0' cellpadding='0' cellspacing='0' width='100%'>
                                    <tr>
                                        <td align='center' style='padding: 0;'>
                                            <table role='presentation' border='0' cellpadding='0' cellspacing='0' style='width: 100%; max-width: 600px; background-color: #d8efd3; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                                                
                                                <!-- Centered Title Section -->
                                                <tr>
                                                    <td style='padding: 30px 30px 20px 30px;' align='center'>
                                                        <h1 style='color: #021207; font-size: 24px; margin: 0;'>Choruko Password Reset</h1>
                                                    </td>
                                                </tr>
                                                <!-- End Centered Title Section -->

                                                <tr>
                                                    <td style='padding: 30px; color: #021207; font-size: 15px; line-height: 24px;'>
                                                        <p style='margin: 0 0 0 0;'>We received a request to reset the password for your account. Click the button below to complete the process:</p>
                                                        
                                                        <table role='presentation' border='0' cellpadding='0' cellspacing='0' style='width: 100%;'>
                                                            <tr>
                                                                <td align='center' style='padding: 32px 0;'>
                                                                    <table border='0' cellpadding='0' cellspacing='0'>
                                                                        <tr>
                                                                            <td align='center' style='border-radius: 10px;' bgcolor='#55ad9b'>
                                                                                <a href='{resetLink}' target='_blank' style='font-size: 16px; font-family: Asap, sans-serif; color: #ffffff; text-decoration: none; border-radius: 10px; padding: 12px 25px; border: 1px solid #55ad9b; display: inline-block; font-weight: bold;'>
                                                                                    Reset Password
                                                                                </a>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                        
                                                        <p style='margin: 0; font-size: 13px; color: #021207;'>If you did not request a password reset, please ignore this email. Your current password will remain unchanged.</p>
                                                    </td>
                                                </tr>
                                                
                                                <tr>
                                                    <td align='center' style='padding: 20px 30px 40px 30px; font-size: 12px; color: #666;'>
                                                        <p style='margin: 0;'>This email was automatically sent by Choruko API.</p>
                                                    </td>
                                                </tr>
                                                
                                            </table>
                                        </td>
                                    </tr>
                                </table>

                            </body>
                            </html>
                        ";

                using (var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpFrom),
                    Subject = "Reset your Choruko password",
                    IsBodyHtml = true,
                    Body = htmlBody,
                })
                {
                    mailMessage.To.Add(email);

                    await client.SendMailAsync(mailMessage);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to send password reset email to {email}. Error: {ex.Message}");
        }
    }
}