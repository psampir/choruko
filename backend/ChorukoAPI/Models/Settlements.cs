using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChorukoAPI.Models;

public class Settlement
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
    [Column(TypeName = "uuid")]
    public string CreatorId{ get; set; }
    [ForeignKey("CreatorId")]
    public virtual User Creator { get; set; }

    [Required]
    [Column(TypeName = "timestamp")]
    public DateTime DateOfCreation{ get; set; }

    [Required]
    [StringLength(500, MinimumLength = 2)]
    [Column(TypeName = "varchar(500)")]
    public string Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    [Required]
    [Column(TypeName = "smallint")]
    public SettlementStatus Status { get; set; }

    public virtual List<User>? SettledUsers { get; set; }
    public virtual List<User>? UnsettledUsers { get; set; }
}
