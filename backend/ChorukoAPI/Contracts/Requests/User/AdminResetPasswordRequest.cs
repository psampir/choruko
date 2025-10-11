using System.ComponentModel.DataAnnotations;

namespace ChorukoAPI.Contracts.Requests.User
{
    public class AdminResetPasswordRequest
    {
        [Required]
        public string DbUsername { get; set; } = string.Empty;
        
        [Required]
        public string DbPassword { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string UserEmail { get; set; } = string.Empty;
    }
}
