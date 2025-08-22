using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Household;
using ChorukoAPI.Contracts.Requests.Task;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.Household;
using ChorukoAPI.Data;
using ChorukoAPI.Extensions;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;

namespace ChorukoAPI.Services.Implementation;

public class HouseholdTaskService(DataContext _context) : IHouseholdTaskService
{
    public async Task<ServiceResponse<OperationResultDto>> CreateHouseholdTaskAsync(string householdId, CreateHouseholdTaskRequest request, string userId)
    {
        try
        {
            var household = await _context.Households
                .Include(h => h.Activities)
                .Include(h => h.Participants)
                .Include(h => h.Tasks).FirstOrDefaultAsync(h => h.Id == householdId);

            var householdTask = new HouseholdTask
            {
                Id = Guid.NewGuid().ToString(),
                Household = household,
                ActivityId = request.ActivityId,
                CreatorId = userId,
                CreationDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                DeadLine = DateTime.SpecifyKind(request.DeadLine.Date, DateTimeKind.Unspecified),
                Status = Models.TaskStatus.Unassigned,
                ParticipantsAmount = request.ParticipantsAmount,
                Reward = request.Reward,
                Name = request.Name,
                Description = request.Description
            };

            var validationResult = await ValidateHouseholdTaskData(householdTask);
            if (validationResult.IsSuccessful == false)
            {
                return validationResult;
            }

            _context.HouseholdTasks.Add(householdTask);
            _context.SaveChanges();

            var response = new OperationResultDto
            {
                Operation = "CreateActivity",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "HouseholdName", household.Name },
                    { "TaskName", householdTask.Name}
                }
            };

            return ServiceResponseFactory.Success(response, "Task for household was successfully created");

        }
        catch(Exception ex)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, $"Failed while creating task: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public Task<ServiceResponse<OperationResultDto>> EditHouseholdTaskAsync(string taskId, EditHouseholdTaskRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResponse<List<HouseholdResponse>>> GetAllHouseholdTaskAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResponse<HouseholdResponse>> GetHouseholdTaskByIdAsync(string activityId)
    {
        throw new NotImplementedException();
    }

    private async Task<ServiceResponse<OperationResultDto>> ValidateHouseholdTaskData(HouseholdTask householdTask)
    {
        var isActivityExist = _context.Activities.Any(a => a.HouseholdId == householdTask.HouseholdId && a.Name == householdTask.Name);

        if (isActivityExist)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Activity with this name already exist", HttpStatusCode.BadRequest);
        }

        return ServiceResponseFactory.Success<OperationResultDto>(null, "Activity is valid for household");
    }
}

