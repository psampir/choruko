using System.ComponentModel.DataAnnotations;

namespace ChorukoAPI.Contracts.Requests.Activity;
public class CreateActivityRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    public int HouseholdId { get; set; }

    [Required]
    public int PointsAwarded { get; set; }
}