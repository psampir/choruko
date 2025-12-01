using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChorukoAPI.Models;

public class Household
{
    [Key]
    [Column(TypeName = "varchar(36)")]
    public string Id { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 2)]
    [Column(TypeName = "varchar(200)")]
    public string Name { get; set; }

    [Required]
    [Column(TypeName = "varchar(36)")]
    public string AdminId { get; set; }
    [ForeignKey("AdminId")]
    public virtual User Admin { get; set; }
    
    [StringLength(500)]
    [Column(TypeName = "varchar(500)")]
    public string? ProfilePicturePath { get; set; }
    [Required]
    [StringLength(3, MinimumLength = 3)]
    [Column(TypeName = "varchar(3)")]
    public string CurrencyCode { get; set; }
    public virtual List<Activity>? Activities { get; set; }
    public virtual List<User> Participants { get; set; }
    public virtual List<HouseholdTask>? Tasks { get; set; }
    public virtual List<Settlement>? Settlements { get; set; }
    public virtual List<Invitation>? Invitations { get; set; }
}
