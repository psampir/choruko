namespace ChorukoAPI.Contracts.Responses.User
{
    public class AdminResetPasswordResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserNewPassword { get; set; } = string.Empty;
    }
}