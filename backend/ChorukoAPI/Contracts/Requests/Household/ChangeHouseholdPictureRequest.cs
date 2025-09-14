using Microsoft.AspNetCore.Http;

namespace ChorukoAPI.Contracts.Requests.Household;

public class ChangeHouseholdPictureRequest
{
    public IFormFile? ProfilePicture { get; set; }
}