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

    [Required]
    [Column(TypeName = "varchar(36)")]
    public string? HouseholdId { get; set; }
    [ForeignKey("HouseholdId")]
    public virtual Household? Household { get; set; } 

    public string? ProfilePicturePath { get; set; }

    public virtual List<HouseholdTask>? AssignedTasks { get; set; }
}