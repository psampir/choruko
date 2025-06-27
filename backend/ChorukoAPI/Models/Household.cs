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

    public virtual List<Activity>? Activities { get; set; }
    public virtual List<User> Participants{ get; set; }
    public virtual List<HouseholdTask>? Tasks{ get; set; }
    public virtual List<Settlement>? Settlements { get; set; }
    public virtual List<Invitation>? Invitations { get; set; }
}
