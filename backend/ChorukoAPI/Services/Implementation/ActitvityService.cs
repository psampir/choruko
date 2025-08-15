using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Activity;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.Activity;
using ChorukoAPI.Data;
using ChorukoAPI.Extensions;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace ChorukoAPI.Services.Implementation
{
    public class ActivityService(DataContext _context) : IActivityService
    {
        public async Task<ServiceResponse<OperationResultDto>> CreateActivityAsync(CreateActivityRequest request)
        {
            var household = await _context.Households.FirstOrDefaultAsync(h => h.Id == request.HouseholdId);

            if (household == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "Household not found.", HttpStatusCode.NotFound);
            }

            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                HouseholdId = household.Id,
                Name = request.Name,
                PointsAwarded = request.PointsAwarded,
            };

            var validationResponse = ValidateActivityData(activity);
            if (!validationResponse.IsSuccessful)
            {
                return validationResponse;
            }

            household.Activities.Add(activity);

            await _context.Activities.AddAsync(activity);
            await _context.SaveChangesAsync();

            var response = new OperationResultDto
            {
                Operation = "CreateActivity",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "HouseholdName", household.Name },
                    { "ActivityName", activity.Name}
                }
            };

            return ServiceResponseFactory.Success(response, "Activity was succesfully created");
        }

        public Task<ServiceResponse<OperationResultDto>> DeleteActivityAsync(int activityId)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<List<ActivityResponse>>> GetActivitiesAsync(string? householdId)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<ActivityResponse>> GetActivityByIdAsync(string activityId)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<OperationResultDto>> UpdateActivityAsync(int activityId, EditActivityRequest request)
        {
            throw new NotImplementedException();
        }

        private ServiceResponse<OperationResultDto> ValidateActivityData(Activity activity)
        {
            var isActivityExist = _context.Activities.Any(a => a.HouseholdId == activity.HouseholdId && a.Name == activity.Name);

            if (isActivityExist)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "Activity with this name already exist", HttpStatusCode.NotFound);
            }

            return ServiceResponseFactory.Success<OperationResultDto>(null, "Activity is valid for household");
        }
    }
}
