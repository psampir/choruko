using ChorukoAPI.Contracts.Responses.Activity;
using ChorukoAPI.Contracts.Responses.Task;
using ChorukoAPI.Contracts.Responses.User;
using ChorukoAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace ChorukoAPI.Mappers
{
    public class HouseholdTaskMapper : IMapper<HouseholdTask, HouseholdTaskResponse>
    {
        public async Task<HouseholdTaskResponse> Map(HouseholdTask source)
        {
            return new HouseholdTaskResponse
            {
                Id = source.Id,
                HouseholdId = source.HouseholdId,
                ActivityId = source.ActivityId,
                Activity = new ActivityResponse
                {
                    Id = source.ActivityId,
                    Name = source.Activity.Name,
                    PointsAwarded = source.Activity.PointsAwarded,
                },
                CreatorId = source.CreatorId,
                Creator = new UserInfoResponse(source.Creator.Id, source.Creator.FirstName, source.Creator.LastName, source.Creator.Email),
                CreationDate = source.CreationDate,
                DeadLine = source.DeadLine,
                Status = source.Status,
                ParticipantsAmount = source.ParticipantsAmount,
                Reward = source.Reward,
                Name = source.Name,
                Description = source.Description,
                Assignees = source.Assignees?.Select(user => new UserInfoResponse(
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email
                )).ToList() ?? new List<UserInfoResponse>()
            };
        }
    }
}
