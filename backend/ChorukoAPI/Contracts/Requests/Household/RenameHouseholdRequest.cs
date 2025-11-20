using System.ComponentModel.DataAnnotations;

namespace ChorukoAPI.Contracts.Requests.Household;

public class RenameHouseholdRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Household name must be between 3 and 100 characters.")]
    public required string NewName { get; set; }
}