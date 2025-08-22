using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Household;
using ChorukoAPI.Contracts.Requests.Task;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.Household;
using ChorukoAPI.Contracts.Responses.Task;
using ChorukoAPI.Data;
using ChorukoAPI.Extensions;
using ChorukoAPI.Mappers;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;

namespace ChorukoAPI.Services.Implementation;

public class HouseholdTaskService(DataContext _context, IMapper<HouseholdTask, HouseholdTaskResponse> _mapper) : IHouseholdTaskService
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
                Operation = "CreateHouseholdTask",
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

    public async Task<ServiceResponse<OperationResultDto>> EditHouseholdTaskAsync(string taskId, EditHouseholdTaskRequest request)
    {
        try
        {
            var householdTask = await _context.HouseholdTasks.FirstOrDefaultAsync(t => t.Id == taskId);

            if (householdTask == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, $"No task were found", HttpStatusCode.NotFound);
            }

            householdTask.ActivityId = request.ActivityId;
            householdTask.DeadLine = DateTime.SpecifyKind(request.DeadLine.Date, DateTimeKind.Unspecified);
            householdTask.ParticipantsAmount = request.ParticipantsAmount;
            householdTask.Reward = request.Reward;
            householdTask.Name = request.Name;
            householdTask.Description = request.Description;

            var validationResult = await ValidateHouseholdTaskData(householdTask);
            if (validationResult.IsSuccessful == false)
            {
                return validationResult;
            }

            _context.SaveChanges();

            var response = new OperationResultDto
            {
                Operation = "EditHouseholdTask",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "TaskName", householdTask.Name}
                }
            };

            return ServiceResponseFactory.Success(response, "Task for household was successfully edited");
        }
        catch (Exception ex)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, $"Failed while editing task: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ServiceResponse<List<HouseholdTaskResponse>>> GetAllHouseholdTaskAsync(string householdId)
    {
        var householdTasks = await _context.HouseholdTasks.Where(t => t.HouseholdId == householdId).ToListAsync();

        if (householdTasks.Count == 0)
        {
            return ServiceResponseFactory.Error<List<HouseholdTaskResponse>>(null, $"No tasks were found", HttpStatusCode.NotFound);
        }

        var response = new List<HouseholdTaskResponse>();

        foreach (var householdTask in householdTasks)
        {
            response.Add(await _mapper.Map(householdTask));
        }

        return ServiceResponseFactory.Success(response, "Success");
    }

    public async Task<ServiceResponse<HouseholdTaskResponse>> GetHouseholdTaskByIdAsync(string householdId)
    {
        var householdTask = await _context.HouseholdTasks.FirstOrDefaultAsync(t => t.Id == householdId);

        if (householdTask == null)
        {
            return ServiceResponseFactory.Error<HouseholdTaskResponse>(null, $"No task were found", HttpStatusCode.NotFound);
        }

        var response = await _mapper.Map(householdTask);

        return ServiceResponseFactory.Success(response, "Success");
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

