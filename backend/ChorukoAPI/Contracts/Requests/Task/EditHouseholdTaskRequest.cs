using System.ComponentModel.DataAnnotations;

namespace ChorukoAPI.Contracts.Requests.Task
{
    public class EditHouseholdTaskRequest
    {
        [Required]
        public string ActivityId { get; set; }

        [Required]
        public DateTime DeadLine { get; set; }

        [Required]
        public int ParticipantsAmount { get; set; }
        
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        public List<string> AssigneeIds { get; set; }
    }
}
