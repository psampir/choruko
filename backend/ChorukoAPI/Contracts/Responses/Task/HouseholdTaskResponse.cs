using ChorukoAPI.Contracts.Responses.Activity;
using ChorukoAPI.Contracts.Responses.User;

namespace ChorukoAPI.Contracts.Responses.Task
{
    public class HouseholdTaskResponse
    {
        public string Id { get; set; }

        public string HouseholdId { get; set; }

        public string ActivityId { get; set; }
        public ActivityResponse Activity { get; set; }

        public string CreatorId { get; set; }
        public UserInfoResponse Creator { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime DeadLine { get; set; }

        public Models.TaskStatus Status { get; set; }

        public int ParticipantsAmount { get; set; }
        
        public string Name { get; set; }

        public string Description { get; set; }

        public List<UserInfoResponse> Assignees { get; set; }
    }
}
