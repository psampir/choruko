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
                .Include(h => h.Tasks)
                .FirstOrDefaultAsync(h => h.Id == householdId);

            if (household == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(
                    null, 
                    $"Household with ID '{householdId}' not found.", 
                    System.Net.HttpStatusCode.NotFound
                );
            }
            
            if (!string.IsNullOrEmpty(request.ActivityId))
            {
                var activityExists = await _context.Activities
                    .AnyAsync(a => a.Id == request.ActivityId && a.HouseholdId == householdId);

                if (!activityExists)
                {
                    return ServiceResponseFactory.Error<OperationResultDto>(
                        null,
                        "The specified ActivityId is invalid or does not belong to this household.",
                        System.Net.HttpStatusCode.BadRequest
                    );
                }
            }
            
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

            return ServiceResponseFactory.Success(response, "Task for household was successfully created.");

        }
        catch(Exception ex)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, $"Failed while creating task: {ex.Message}.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ServiceResponse<OperationResultDto>> EditHouseholdTaskAsync(string taskId, EditHouseholdTaskRequest request)
    {
        try
        {
            var householdTask = await _context.HouseholdTasks.FirstOrDefaultAsync(t => t.Id == taskId);

            if (householdTask == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, $"No tasks were found.", HttpStatusCode.NotFound);
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

            return ServiceResponseFactory.Success(response, "Task for household was successfully edited.");
        }
        catch (Exception ex)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, $"Failed while editing task: {ex.Message}.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ServiceResponse<List<HouseholdTaskResponse>>> GetAllHouseholdTaskAsync(string householdId)
    {
        var householdTasks = await _context.HouseholdTasks.Where(t => t.HouseholdId == householdId).ToListAsync();

        if (householdTasks.Count == 0)
        {
            return ServiceResponseFactory.Error<List<HouseholdTaskResponse>>(null, $"No tasks were found.", HttpStatusCode.NotFound);
        }

        var response = new List<HouseholdTaskResponse>();

        foreach (var householdTask in householdTasks)
        {
            response.Add(await _mapper.Map(householdTask));
        }

        return ServiceResponseFactory.Success(response, "Success.");
    }

    public async Task<ServiceResponse<HouseholdTaskResponse>> GetHouseholdTaskByIdAsync(string taskId)
    {
        var householdTask = await _context.HouseholdTasks.FirstOrDefaultAsync(t => t.Id == taskId);

        if (householdTask == null)
        {
            return ServiceResponseFactory.Error<HouseholdTaskResponse>(null, $"No tasks were found.", HttpStatusCode.NotFound);
        }

        var response = await _mapper.Map(householdTask);

        return ServiceResponseFactory.Success(response, "Success.");
    }

    public async Task<ServiceResponse<OperationResultDto>> AssignUserToTask(string taskId, string userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        var householdTask = await _context.HouseholdTasks.FirstOrDefaultAsync(h => h.Id == taskId);
        if (householdTask == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, $"No tasks were found.", HttpStatusCode.NotFound);
        }

        if (householdTask.Status == Models.TaskStatus.Assigned)
        {
            user.AssignedTasks.Add(householdTask);
            _context.SaveChanges();
        }

        householdTask.Status = Models.TaskStatus.Assigned;

        user.AssignedTasks.Add(householdTask);
        _context.SaveChanges();

        var response = new OperationResultDto
        {
            Operation = "EditHouseholdTask",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
                {
                    { "TaskName", householdTask.Name},
                    { "UserName", user.FirstName + " " + user.LastName },
                    { "Assigned", true }
                }
        };

        return ServiceResponseFactory.Success(response, "User successfully assigned to household task.");
    }

    public async Task<ServiceResponse<OperationResultDto>> UnAssignUserFromTask(string taskId, string userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        var householdTask = await _context.HouseholdTasks.FirstOrDefaultAsync(h => h.Id == taskId);
        if (householdTask == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, $"No tasks were found.", HttpStatusCode.NotFound);
        }

        if (householdTask.Assignees.Count < 1)
        {
            householdTask.Status = Models.TaskStatus.Unassigned;
            user.AssignedTasks.Remove(householdTask);
            _context.SaveChanges();
        }
        else
        {
            user.AssignedTasks.Remove(householdTask);
            _context.SaveChanges();
        }

        var response = new OperationResultDto
        {
            Operation = "EditHouseholdTask",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
                {
                    { "TaskName", householdTask.Name},
                    { "UserName", user.FirstName + " " + user.LastName },
                    { "UnAssigned", true }
                }
        };

        return ServiceResponseFactory.Success(response, "User successfully unassigned from household task.");
    }

    public async Task<ServiceResponse<OperationResultDto>> MarkTaskAsCompleted(string taskId)
    {
        var householdTask = await _context.HouseholdTasks
            .Include(t => t.Assignees) 
            .FirstOrDefaultAsync(h => h.Id == taskId);
        
        if (householdTask == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, $"No tasks were found.", HttpStatusCode.NotFound);
        }

        int rewardPoints = householdTask.Reward;
        
        foreach (var assignee in householdTask.Assignees)
        {
            assignee.PointsCurrentMonth += rewardPoints;
            assignee.PointsCurrentYear += rewardPoints;
            assignee.PointsAllTime += rewardPoints;
            
            _context.Users.Update(assignee);
        }
        
        householdTask.Status = Models.TaskStatus.Completed;
        _context.SaveChanges();

        var response = new OperationResultDto
        {
            Operation = "EditHouseholdTask",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
                {
                    { "TaskName", householdTask.Name},
                    { "Completed", true }
                }
        };

        return ServiceResponseFactory.Success(response, "Task successfully completed.");
    }

    public async Task<ServiceResponse<OperationResultDto>> UnMarkTaskAsCompleted(string taskId)
    {
        var householdTask = await _context.HouseholdTasks
            .Include(h => h.Assignees)
            .FirstOrDefaultAsync(h => h.Id == taskId);
        
        if (householdTask == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, $"No tasks were found.", HttpStatusCode.NotFound);
        }
        
        if (householdTask.Status != Models.TaskStatus.Completed)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, $"Task is not currently marked as completed.", HttpStatusCode.BadRequest);
        }
        
        int rewardPoints = householdTask.Reward;

        if (householdTask.Assignees != null && householdTask.Assignees.Count > 0)
        {
            foreach (var assignee in householdTask.Assignees)
            {
                assignee.PointsCurrentMonth = Math.Max(0, assignee.PointsCurrentMonth - rewardPoints);
                assignee.PointsCurrentYear = Math.Max(0, assignee.PointsCurrentYear - rewardPoints);
                assignee.PointsAllTime = Math.Max(0, assignee.PointsAllTime - rewardPoints);
                
                _context.Users.Update(assignee);
            }
        }
        
        if (householdTask.Assignees == null || householdTask.Assignees.Count == 0)
        {
            householdTask.Status = Models.TaskStatus.Unassigned;
        }
        else
        {
            householdTask.Status = Models.TaskStatus.Assigned;
        }
        
        _context.SaveChanges();

        var response = new OperationResultDto
        {
            Operation = "EditHouseholdTask",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
                {
                    { "TaskName", householdTask.Name},
                    { "Completed", false }
                }
        };

        return ServiceResponseFactory.Success(response, "Task successfully uncompleted.");
    }

    public async Task<ServiceResponse<OperationResultDto>> DeleteHouseholdTask(string taskId)
    {
        var householdTask = await _context.HouseholdTasks.FirstOrDefaultAsync(h => h.Id == taskId);
        if (householdTask == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, $"No tasks were found.", HttpStatusCode.NotFound);
        }

        _context.HouseholdTasks.Remove(householdTask);
        _context.SaveChanges();

        var response = new OperationResultDto
        {
            Operation = "DeleteHouseholdTask",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
                {
                    { "TaskName", householdTask.Name}
                }
        };

        return ServiceResponseFactory.Success(response, "Activity was successfully created.");
    }

    private async Task<ServiceResponse<OperationResultDto>> ValidateHouseholdTaskData(HouseholdTask householdTask)
    {
        var isActivityExist = _context.Activities.Any(a => a.HouseholdId == householdTask.HouseholdId && a.Name == householdTask.Name);

        if (isActivityExist)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Activity with this name already exist.", HttpStatusCode.BadRequest);
        }

        return ServiceResponseFactory.Success<OperationResultDto>(null, "Activity is valid for household.");
    }
}

