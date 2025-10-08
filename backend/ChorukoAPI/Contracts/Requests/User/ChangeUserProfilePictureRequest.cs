using Microsoft.AspNetCore.Http;

namespace ChorukoAPI.Contracts.Requests.User;

public class ChangeUserProfilePictureRequest
{
    public IFormFile? ProfilePicture { get; set; }
}