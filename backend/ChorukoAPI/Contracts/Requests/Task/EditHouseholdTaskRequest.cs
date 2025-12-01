using System.ComponentModel.DataAnnotations;

namespace ChorukoAPI.Contracts.Requests.Task
{
    public class EditHouseholdTaskRequest
    {
        [Required]
        public required string ActivityId { get; set; }

        [Required]
        public DateTime DeadLine { get; set; }

        [Required]
        public int ParticipantsAmount { get; set; }
        
        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Description { get; set; }

        public required List<string> AssigneeIds { get; set; }
    }
}
