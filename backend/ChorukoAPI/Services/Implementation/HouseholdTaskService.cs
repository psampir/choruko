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
using Org.BouncyCastle.Asn1.Ocsp;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;

namespace ChorukoAPI.Services.Implementation;

public class HouseholdTaskService(DataContext _context, IMapper<HouseholdTask, HouseholdTaskResponse> _mapper, IPushNotificationService pushNotificationService) : IHouseholdTaskService
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

            var assigneeIds = request.AssigneeIds ?? new List<string>();

            var usersToAdd = household.Participants.Where(p => assigneeIds.Contains(p.Id)).ToList();

            var householdTask = new HouseholdTask();
            if (usersToAdd.Count == 0)
            {
                householdTask = new HouseholdTask
                {
                    Id = Guid.NewGuid().ToString(),
                    Household = household,
                    ActivityId = request.ActivityId,
                    CreatorId = userId,
                    CreationDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    DeadLine = DateTime.SpecifyKind(request.DeadLine, DateTimeKind.Unspecified),
                    Status = Models.TaskStatus.Unassigned,
                    ParticipantsAmount = request.ParticipantsAmount,
                    Name = request.Name,
                    Description = request.Description
                };
            }
            else
            {
                householdTask = new HouseholdTask
                {
                    Id = Guid.NewGuid().ToString(),
                    Household = household,
                    ActivityId = request.ActivityId,
                    CreatorId = userId,
                    CreationDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    DeadLine = DateTime.SpecifyKind(request.DeadLine, DateTimeKind.Unspecified),
                    Status = Models.TaskStatus.Assigned,
                    ParticipantsAmount = request.ParticipantsAmount,
                    Name = request.Name,
                    Description = request.Description,
                    Assignees = usersToAdd,
                };
            }

            var validationResult = await ValidateHouseholdTaskData(householdTask);
            if (validationResult.IsSuccessful == false)
            {
                return validationResult;
            }

            _context.HouseholdTasks.Add(householdTask);
            await _context.SaveChangesAsync();

            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                title = "👋 New task was created!",
                body = $"{request.Description}, deadline:{request.DeadLine}!",
                icon = "/icons/icon-192x192.png",
                badge = "/icons/icon-96x96.png",
                url = "/"
            });

            await NotificationHandler.SendNotificationAsync(household.Participants.Select(receiver => receiver.Id).ToList(), payload);

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
            var householdTask = await _context.HouseholdTasks.Include(t => t.Assignees).FirstOrDefaultAsync(t => t.Id == taskId);

            if (householdTask == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, $"No tasks were found.", HttpStatusCode.NotFound);
            }

            // Prevent editing completed tasks
            if (householdTask.Status == Models.TaskStatus.Completed)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, $"Cannot edit a completed task. Please mark it as uncompleted first.", HttpStatusCode.BadRequest);
            }

            var household = await _context.Households
                .Include(h => h.Activities)
                .Include(h => h.Participants)
                .Include(h => h.Tasks)
                .FirstOrDefaultAsync(h => h.Id == householdTask.HouseholdId);

            var assigneeIds = request.AssigneeIds ?? new List<string>();

            var usersToAdd = household.Participants.Where(p => assigneeIds.Contains(p.Id)).ToList();

            householdTask.Assignees = usersToAdd;
            householdTask.ActivityId = request.ActivityId;
            householdTask.DeadLine = DateTime.SpecifyKind(request.DeadLine, DateTimeKind.Unspecified);
            householdTask.ParticipantsAmount = request.ParticipantsAmount;
            householdTask.Name = request.Name;
            householdTask.Description = request.Description;
            householdTask.Status = usersToAdd.Any() ? Models.TaskStatus.Assigned : Models.TaskStatus.Unassigned;

            var validationResult = await ValidateHouseholdTaskData(householdTask);
            if (validationResult.IsSuccessful == false)
            {
                return validationResult;
            }

            await _context.SaveChangesAsync();

            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                title = "👋 Task info was edited!",
                body = $"{request.Description}, deadline:{request.DeadLine}!",
                icon = "/icons/icon-192x192.png",
                badge = "/icons/icon-96x96.png",
                url = "/"
            });

            await NotificationHandler.SendNotificationAsync(householdTask.Assignees.Select(receiver => receiver.Id).ToList(), payload);

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
        var user = await _context.Users
            .Include(u => u.AssignedTasks)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "User not found.", HttpStatusCode.NotFound);
        }

        var householdTask = await _context.HouseholdTasks
            .Include(t => t.Assignees)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (householdTask == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Task not found.", HttpStatusCode.NotFound);
        }

        // Prevent assigning to completed tasks
        if (householdTask.Status == Models.TaskStatus.Completed)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Cannot assign to a completed task. Please mark it as uncompleted first.", HttpStatusCode.BadRequest);
        }

        householdTask.Status = Models.TaskStatus.Assigned;

        if (!user.AssignedTasks.Contains(householdTask))
        {
            user.AssignedTasks.Add(householdTask);
        }

        await _context.SaveChangesAsync();

        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "👋 New assignee in task!",
            body = $"{householdTask.Description}, assignee:{user.FirstName} {user.LastName}!",
            icon = "/icons/icon-192x192.png",
            badge = "/icons/icon-96x96.png",
            url = "/"
        });

        await pushNotificationService.SendNotificationAsync(householdTask.CreatorId, payload);

        var response = new OperationResultDto
        {
            Operation = "AssignUserToTask",
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
        var user = await _context.Users
            .Include(u => u.AssignedTasks)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "User not found.", HttpStatusCode.NotFound);
        }

        var householdTask = await _context.HouseholdTasks
            .Include(t => t.Assignees)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (householdTask == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Task not found.", HttpStatusCode.NotFound);
        }

        // Prevent unassigning from completed tasks
        if (householdTask.Status == Models.TaskStatus.Completed)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Cannot unassign from a completed task. Please mark it as uncompleted first.", HttpStatusCode.BadRequest);
        }

        user.AssignedTasks.Remove(householdTask);

        if (householdTask.Assignees.Count == 0)
        {
            householdTask.Status = Models.TaskStatus.Unassigned;
        }

        await _context.SaveChangesAsync();

        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "👋 User left task!",
            body = $"{householdTask.Description}, assignee:{user.FirstName} {user.LastName}!",
            icon = "/icons/icon-192x192.png",
            badge = "/icons/icon-96x96.png",
            url = "/"
        });

        await pushNotificationService.SendNotificationAsync(householdTask.CreatorId, payload);

        var response = new OperationResultDto
        {
            Operation = "UnAssignUserFromTask",
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
            .Include(t => t.Activity)
            .FirstOrDefaultAsync(h => h.Id == taskId);
        
        if (householdTask == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, $"No tasks were found.", HttpStatusCode.NotFound);
        }
        
        int rewardPoints = householdTask.Activity.PointsAwarded;
        
        foreach (var assignee in householdTask.Assignees)
        {
            assignee.PointsCurrentMonth += rewardPoints;
            assignee.PointsCurrentYear += rewardPoints;
            assignee.PointsAllTime += rewardPoints;
            
            _context.Users.Update(assignee);
        }
        
        householdTask.Status = Models.TaskStatus.Completed;
        await _context.SaveChangesAsync();

        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "👋 Task was completed!",
            body = $"{householdTask.Description}!",
            icon = "/icons/icon-192x192.png",
            badge = "/icons/icon-96x96.png",
            url = "/"
        });

        await NotificationHandler.SendNotificationAsync(householdTask.Assignees.Select(receiver => receiver.Id).ToList(), payload);

        await pushNotificationService.SendNotificationAsync(householdTask.CreatorId, payload);

        var response = new OperationResultDto
        {
            Operation = "CompleteHouseholdTask",
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
            .Include(h => h.Activity)
            .FirstOrDefaultAsync(h => h.Id == taskId);
        
        if (householdTask == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, $"No tasks were found.", HttpStatusCode.NotFound);
        }
        
        if (householdTask.Status != Models.TaskStatus.Completed)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, $"Task is not currently marked as completed.", HttpStatusCode.BadRequest);
        }
        
        int rewardPoints = householdTask.Activity.PointsAwarded;

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
            await _context.SaveChangesAsync();
        }
        else
        {
            householdTask.Status = Models.TaskStatus.Assigned;
            await _context.SaveChangesAsync();
        }

        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "👋 Task was opened!",
            body = $"{householdTask.Description}!",
            icon = "/icons/icon-192x192.png",
            badge = "/icons/icon-96x96.png",
            url = "/"
        });

        await NotificationHandler.SendNotificationAsync(householdTask.Assignees.Select(receiver => receiver.Id).ToList(), payload);

        await pushNotificationService.SendNotificationAsync(householdTask.CreatorId, payload);

        var response = new OperationResultDto
        {
            Operation = "CompleteHouseholdTask",
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
        var householdTask = await _context.HouseholdTasks.Include(t => t.Assignees).FirstOrDefaultAsync(t => t.Id == taskId);
        if (householdTask == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, $"No tasks were found.", HttpStatusCode.NotFound);
        }

        // Prevent deleting completed tasks
        if (householdTask.Status == Models.TaskStatus.Completed)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Cannot delete a completed task. Please mark it as uncompleted first.", HttpStatusCode.BadRequest);
        }

        _context.HouseholdTasks.Remove(householdTask);
        await _context.SaveChangesAsync();

        var response = new OperationResultDto
        {
            Operation = "DeleteHouseholdTask",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
                {
                    { "TaskName", householdTask.Name}
                }
        };

        return ServiceResponseFactory.Success(response, "Task was successfully deleted.");
    }

    private async Task<ServiceResponse<OperationResultDto>> ValidateHouseholdTaskData(HouseholdTask householdTask)
    {
        var isTaskNameTaken = await _context.HouseholdTasks.AnyAsync(t => t.HouseholdId == householdTask.Household.Id && t.Name == householdTask.Name && t.Id != householdTask.Id);

        if (isTaskNameTaken)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "A task with this name already exists in this household.", HttpStatusCode.BadRequest);
        }

        if (!CheckParticipantAmount(householdTask))
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, 
                $"Too many participants assigned. The limit for this task is {householdTask.ParticipantsAmount}, but {householdTask.Assignees.Count} were provided.",
                HttpStatusCode.BadRequest);
        }

        return ServiceResponseFactory.Success<OperationResultDto>(null, "Task data is valid.");
    }

    private bool CheckParticipantAmount(HouseholdTask householdTask)
    {
        var assigneeCount = householdTask.Assignees?.Count ?? 0;

        return assigneeCount <= householdTask.ParticipantsAmount;
    }
}

