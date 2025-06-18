using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ChorukoServer.Models;

public class User : IdentityUser
{
    [Required]
    [StringLength(256)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(256)]
    public string LastName { get; set; }

    public string? HouseholdId { get; set; }
    public virtual Household? Household { get; set; } 

    public string? ProfilePicturePath { get; set; }

    public virtual List<HouseholdTask>? assignedTasks { get; set; }

}