using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChorukoAPI.Models;

public class TaskReward
{
    [Key]
    [Column(TypeName = "uuid")]

    public string UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User User { get; set; }

    [Required]
    [Column(TypeName = "integer")]
    public int Points { get; set; }

    [Required]
    [Column(TypeName = "timestamp")]
    public DateTime Date { get; set; }
}
