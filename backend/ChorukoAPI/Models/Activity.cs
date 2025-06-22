using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChorukoAPI.Models;

public class Activity
{
    [Key]
    [Column(TypeName = "uuid")]
    public string Id { get; set; }

    [Required]
    [Column(TypeName = "uuid")]
    public string HouseholdId { get; set; }
    [ForeignKey("HouseholdId")]
    public virtual Household Household { get; set; }

    [Required]
    [StringLength(200)]
    [Column(TypeName = "varchar(200)")]
    public string Name { get; set; }

    [Required]
    [Column(TypeName = "integer")]
    public int PointsAwarded { get; set; }
    public virtual List<HouseholdTask> Tasks { get; set; }
}
