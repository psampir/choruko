using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Activity;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.Activity;
using ChorukoAPI.Data;
using ChorukoAPI.Extensions;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Net;

namespace ChorukoAPI.Services.Implementation
{
    public class ActivityService(DataContext _context) : IActivityService
    {
        public async Task<ServiceResponse<OperationResultDto>> CreateActivityAsync(string householdId, CreateActivityRequest request)
        {
            var household = await _context.Households.FirstOrDefaultAsync(h => h.Id == householdId);

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

            return ServiceResponseFactory.Success(response, "Activity was successfully created");
        }

        public async Task<ServiceResponse<OperationResultDto>> EditActivityAsync(string activityId, EditActivityRequest request)
        {
            var activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == activityId);
            if (activity == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "Activity not found.", HttpStatusCode.NotFound);
            }

            activity.Name = request.Name;
            activity.PointsAwarded = request.PointsAwarded;

            var validationResponse = ValidateActivityData(activity);
            if (!validationResponse.IsSuccessful)
            {
                return validationResponse;
            }

            await _context.SaveChangesAsync();

            var response = new OperationResultDto
            {
                Operation = "EditActivity",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "ActivityName", activity.Name}
                }
            };

            return ServiceResponseFactory.Success(response);
        }

        public async Task<ServiceResponse<List<ActivityResponse>>> GetActivitiesAsync(string? householdId)
        {
            var householdActivities = await _context.Activities.Where(a => a.HouseholdId == householdId).ToListAsync();
            if (householdActivities.Count == 0)
            {
                return ServiceResponseFactory.Error<List<ActivityResponse>>(null, "No activities found in specified household.", HttpStatusCode.NotFound);
            }

            var response = new List<ActivityResponse>();

            foreach(var activity in householdActivities)
            {
                response.Add(new ActivityResponse
                {
                    Id = activity.Id,
                    Name = activity.Name,
                    PointsAwarded = activity.PointsAwarded,
                });
            }

            return ServiceResponseFactory.Success(response, "Successful extraction of household activities");
        }

        public async Task<ServiceResponse<ActivityResponse>> GetActivityByIdAsync(string activityId)
        {
            var householdActivity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == activityId);
            if (householdActivity == null)
            {
                return ServiceResponseFactory.Error<ActivityResponse>(null, "No activity found in household.", HttpStatusCode.NotFound);
            }

            var response = new ActivityResponse
            {
                Id = householdActivity.Id,
                Name = householdActivity.Name,
                PointsAwarded = householdActivity.PointsAwarded,
            };

            return ServiceResponseFactory.Success(response, "Successful extraction of household activity");
        }

        public async Task<ServiceResponse<OperationResultDto>> DeleteActivityAsync(string activityId)
        {
            var householdActivity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == activityId);
            if (householdActivity == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "No activity found in household.", HttpStatusCode.NotFound);
            }

            _context.Activities.Remove(householdActivity);
            await _context.SaveChangesAsync();

            var response = new OperationResultDto
            {
                Operation = "DeleteActivity",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "ActivityName", householdActivity.Name}
                }
            };

            return ServiceResponseFactory.Success(response, "Activity was successfully created");
        }

        private ServiceResponse<OperationResultDto> ValidateActivityData(Activity activity)
        {
            var isActivityExist = _context.Activities.Any(a => a.HouseholdId == activity.HouseholdId && a.Name == activity.Name);

            if (isActivityExist)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "Activity with this name already exist", HttpStatusCode.BadRequest);
            }

            return ServiceResponseFactory.Success<OperationResultDto>(null, "Activity is valid for household");
        }
    }
}
