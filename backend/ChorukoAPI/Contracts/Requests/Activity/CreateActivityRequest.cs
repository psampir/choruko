using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ChorukoAPI.Contracts.Requests.Activity;

public class CreateActivityRequest
{
    [Required(ErrorMessage = "Activity name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Activity name must be at least {2} characters long.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Points awarded is required")]
    [Range(1, 99, ErrorMessage = "Points awarded must be between {1} and {2}.")]
    public int PointsAwarded { get; set; }
}