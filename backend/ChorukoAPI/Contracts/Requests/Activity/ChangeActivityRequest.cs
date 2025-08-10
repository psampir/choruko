using System.ComponentModel.DataAnnotations;

namespace ChorukoAPI.Contracts.Requests.Activity;
public class ChangeActivityRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    public int HouseholdId { get; set; }

    [Required]
    public int PointsAwarded { get; set; }
}
