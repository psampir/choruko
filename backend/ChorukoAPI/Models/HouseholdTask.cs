using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChorukoAPI.Models;

public class HouseholdTask
{
    [Key]
    [Column(TypeName = "varchar(36)")]
    public string Id { get; set; }

    [Required]
    [Column(TypeName = "varchar(36)")]
    public string HouseholdId { get; set; }
    [ForeignKey("HouseholdId")]
    public virtual Household Household { get; set; }

    [Column(TypeName = "varchar(36)")]
    public string ActivityId { get; set; }
    [ForeignKey("ActivityId")]
    public virtual Activity Activity { get; set; }

    [Required]
    [Column(TypeName = "varchar(36)")]
    public string CreatorId { get; set; }
    [ForeignKey("CreatorId")]
    public virtual User Creator { get; set; }

    [Required]
    [Column(TypeName = "timestamp")]
    public DateTime CreationDate { get; set; }
    [Required]
    [Column(TypeName = "timestamp")]
    public DateTime DeadLine { get; set; }

    [Required]
    [Column(TypeName = "smallint")]
    public TaskStatus Status { get; set; }

    [Required]
    [Range(1, 100)]
    [Column(TypeName = "integer")]
    public int ParticipantsAmount { get; set; }

    [Required]
    [Range(1, 100)]
    [Column(TypeName = "integer")]
    public int Reward{ get; set; }

    [Required]
    [StringLength(200)]
    [Column(TypeName = "varchar(200)")]
    public string Name { get; set; }

    [StringLength(1000)]
    [Column(TypeName = "text")]
    public string Description { get; set; }

    public virtual List<User>? Assignees { get; set; }
}