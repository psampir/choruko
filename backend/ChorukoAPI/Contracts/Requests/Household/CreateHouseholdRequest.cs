namespace ChorukoAPI.Contracts.Requests.Household;

using Microsoft.AspNetCore.Http; // Required for IFormFile
using System.ComponentModel.DataAnnotations;

public class CreateHouseholdRequest
{
    [Required(ErrorMessage = "Household name is required.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Household name must be between 2 and 200 characters.")]
    public required string Name { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    
    public IFormFile? ProfilePicture { get; set; }
}
