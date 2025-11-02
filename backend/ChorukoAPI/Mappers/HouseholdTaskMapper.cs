using ChorukoAPI.Contracts.Responses.Activity;
using ChorukoAPI.Contracts.Responses.Task;
using ChorukoAPI.Contracts.Responses.User;
using ChorukoAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace ChorukoAPI.Mappers
{
    public class HouseholdTaskMapper(IMapper<User, UserInfoResponse> userMapper) : IMapper<HouseholdTask, HouseholdTaskResponse>
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
                Creator = await userMapper.Map(source.Creator),
                CreationDate = source.CreationDate,
                DeadLine = source.DeadLine,
                Status = source.Status,
                ParticipantsAmount = source.ParticipantsAmount,
                Name = source.Name,
                Description = source.Description,
                Assignees = (await Task.WhenAll(source.Assignees.Select(x => userMapper.Map(x)))).ToList() ?? new List<UserInfoResponse>()
            };
        }
    }
}
