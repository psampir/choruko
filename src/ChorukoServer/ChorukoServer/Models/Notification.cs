using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChorukoServer.Models;

public class Notification
{
    [Key]
    [Column(TypeName = "uuid")]
    public string Id { get; set; }

    [Required]
    [Column(TypeName = "uuid")]
    public string ReceiverId { get; set; }
    [ForeignKey("ReceiverId")]
    public virtual User Receiver { get; set; }

    [Required]
    [Column(TypeName = "smallint")]
    public NotificationType NotificationType { get; set; }

    [Required]
    [StringLength(500)]
    [Column(TypeName = "varchar(500)")]
    public string Message { get; set; }

    [Required]
    [Column(TypeName = "timestamp")]
    public DateTime SentAt { get; set; }

    [Column(TypeName = "uuid")]
    public string? SettlementId { get; set; }
    [ForeignKey("SettlementId")]
    public virtual Settlement? Settlement { get; set; }

    [Column(TypeName = "uuid")]
    public string? HouseholdTaskId { get; set; }
    [ForeignKey("HouseholdTaskId")]
    public virtual HouseholdTask? HouseholdTask { get; set; }

    [Column(TypeName = "uuid")]
    public string? InvitationId { get; set; }
    [ForeignKey("InvitationId")]
    public virtual Invitation? Invitation { get; set; }
}
