using Microsoft.EntityFrameworkCore.Diagnostics;
using System.ComponentModel.DataAnnotations;

namespace ChorukoAPI.Contracts.Requests.Task
{
    public class CreateHouseholdTaskRequest
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
    }
}
