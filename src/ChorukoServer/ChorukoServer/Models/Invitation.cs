using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChorukoServer.Models;

public class Invitation
{
    [Key]
    [Column(TypeName = "uuid")]
    public string Id { get; set; }

    [Required]
    [Column(TypeName = "uuid")]
    public string SenderId { get; set; }
    [ForeignKey("SenderId")]
    public virtual User Sender { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string RecieverEmail { get; set; }

    [Required]
    [Column(TypeName = "uuid")]
    public string HouseholdId { get; set; }
    [ForeignKey("HouseholdId")]
    public virtual Household Household { get; set; }

    [Required]
    [Column(TypeName = "smallint")]
    public InvitationStatus Status { get; set; }

    [Required]
    [Column(TypeName = "timestamp")]
    public DateTime SentAt { get; set; }
}
