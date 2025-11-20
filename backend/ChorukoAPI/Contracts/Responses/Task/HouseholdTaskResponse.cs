using ChorukoAPI.Contracts.Responses.Activity;
using ChorukoAPI.Contracts.Responses.User;

namespace ChorukoAPI.Contracts.Responses.Task
{
    public class HouseholdTaskResponse
    {
        public required string Id { get; set; }

        public required string HouseholdId { get; set; }

        public required string ActivityId { get; set; }
        public required ActivityResponse Activity { get; set; }

        public required string CreatorId { get; set; }
        public required UserInfoResponse Creator { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime DeadLine { get; set; }

        public Models.TaskStatus Status { get; set; }

        public int ParticipantsAmount { get; set; }
        
        public required string Name { get; set; }

        public required string Description { get; set; }

        public required List<UserInfoResponse> Assignees { get; set; }
    }
}
