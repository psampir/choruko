using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChorukoAPI.Models;

public class User : IdentityUser
{
    [Required]
    [StringLength(256)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(256)]
    public string LastName { get; set; }

    [Column(TypeName = "varchar(36)")]
    public string? HouseholdId { get; set; }
    [ForeignKey("HouseholdId")]
    public virtual Household? Household { get; set; }

    public string? ProfilePicturePath { get; set; }

    [StringLength(64)]
    [Column(TypeName = "varchar(64)")]
    public string? NtfyTopic { get; set; }

    public virtual List<HouseholdTask>? AssignedTasks { get; set; }
    
    public int PointsCurrentMonth { get; set; }
    
    public int PointsCurrentYear { get; set; }
    
    public int PointsAllTime { get; set; }
}
