using System.ComponentModel.DataAnnotations;

namespace ChorukoAPI.Contracts.Requests.Activity;

public class UpdateActivityRequest
{
    [Required(ErrorMessage = "Activity name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Activity name must be at least {2} characters long.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Household ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Household ID is required")]
    public int HouseholdId { get; set; }

    [Required(ErrorMessage = "Points awarded is required")]
    [Range(1, 1000, ErrorMessage = "Points awarded must be at least {2} characters long.")]
    public int PointsAwarded { get; set; }
}
