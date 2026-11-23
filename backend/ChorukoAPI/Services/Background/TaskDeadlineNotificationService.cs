using ChorukoAPI.Contracts.Notifications;
using ChorukoAPI.Data;
using ChorukoAPI.Extensions;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace ChorukoAPI.Services.Background;

public class TaskDeadlineNotificationService(
    ILogger<TaskDeadlineNotificationService> logger,
    IServiceScopeFactory scopeFactory,
    INotifier notifier) : BackgroundService
{
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);
    private readonly HashSet<string> _notifiedTasksReminder = [];
    private readonly HashSet<string> _notifiedTasksOverdue = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Task Deadline Notification Service started. Checking every hour for task deadlines.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_checkInterval, stoppingToken);

            try
            {
                await CheckAndNotifyTaskDeadlines(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Task Deadline Notification Service");
            }
        }
    }

    private async Task CheckAndNotifyTaskDeadlines(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

        // Tier 1: Find tasks due in 23-24 hours (advance reminder)
        var reminderLowerBound = now.AddHours(23);
        var reminderUpperBound = now.AddHours(24);

        var tasksDueSoon = await context.HouseholdTasks
            .Include(t => t.Assignees)
            .Include(t => t.Creator)
            .Where(t => t.Status != Models.TaskStatus.Completed
                && t.DeadLine >= reminderLowerBound
                && t.DeadLine <= reminderUpperBound
                && !_notifiedTasksReminder.Contains(t.Id))
            .ToListAsync(stoppingToken);

        // Tier 2: Find tasks that passed deadline in the last hour (overdue alert)
        var overdueLowerBound = now.AddHours(-1);

        var tasksOverdue = await context.HouseholdTasks
            .Include(t => t.Assignees)
            .Include(t => t.Creator)
            .Where(t => t.Status != Models.TaskStatus.Completed
                && t.DeadLine <= now
                && t.DeadLine >= overdueLowerBound
                && !_notifiedTasksOverdue.Contains(t.Id))
            .ToListAsync(stoppingToken);

        // Send reminder notifications
        if (tasksDueSoon.Count > 0)
        {
            logger.LogInformation("Found {Count} task(s) due in 24 hours", tasksDueSoon.Count);

            foreach (var task in tasksDueSoon)
            {
                try
                {
                    await SendReminderNotification(task);
                    _notifiedTasksReminder.Add(task.Id);
                    logger.LogInformation("Sent reminder for task: {TaskName} (ID: {TaskId})", task.Name, task.Id);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send reminder for task {TaskId}", task.Id);
                }
            }
        }

        // Send overdue notifications
        if (tasksOverdue.Count > 0)
        {
            logger.LogInformation("Found {Count} task(s) that are now overdue", tasksOverdue.Count);

            foreach (var task in tasksOverdue)
            {
                try
                {
                    await SendOverdueNotification(task);
                    _notifiedTasksOverdue.Add(task.Id);
                    logger.LogInformation("Sent overdue alert for task: {TaskName} (ID: {TaskId})", task.Name, task.Id);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send overdue notification for task {TaskId}", task.Id);
                }
            }
        }

        if (tasksDueSoon.Count == 0 && tasksOverdue.Count == 0)
        {
            logger.LogInformation("No tasks requiring notifications found.");
        }

        // Clean up completed tasks from tracking
        await CleanupNotifiedTasks(context, stoppingToken);
    }

    private async Task SendReminderNotification(HouseholdTask task)
    {
        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "⏰ Task due tomorrow",
            body = $"{task.Name}",
            icon = "/icons/icon-192x192.png",
            badge = "/icons/icon-96x96.png",
            url = "/tasks"
        });

        await SendNotificationToTaskParticipants(task, payload);
    }

    private async Task SendOverdueNotification(HouseholdTask task)
    {
        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "⚠️ Task overdue",
            body = $"{task.Name}",
            icon = "/icons/icon-192x192.png",
            badge = "/icons/icon-96x96.png",
            url = "/tasks"
        });

        await SendNotificationToTaskParticipants(task, payload);
    }

    private async Task SendNotificationToTaskParticipants(HouseholdTask task, string payload)
    {
        // Collect all users to notify: assignees + creator
        var userIdsToNotify = new HashSet<string>();

        // Add assignees
        if (task.Assignees != null && task.Assignees.Count > 0)
        {
            foreach (var assignee in task.Assignees)
            {
                userIdsToNotify.Add(assignee.Id);
            }
        }

        // Add creator
        userIdsToNotify.Add(task.CreatorId);

        // Send notifications
        if (userIdsToNotify.Count > 0)
        {
            await notifier.SendAsync(userIdsToNotify, AppNotification.FromJson(payload));
        }
    }

    private async Task CleanupNotifiedTasks(DataContext context, CancellationToken stoppingToken)
    {
        // Remove completed tasks from both tracking sets
        var completedTaskIds = await context.HouseholdTasks
            .Where(t => (_notifiedTasksReminder.Contains(t.Id) || _notifiedTasksOverdue.Contains(t.Id))
                && t.Status == Models.TaskStatus.Completed)
            .Select(t => t.Id)
            .ToListAsync(stoppingToken);

        foreach (var taskId in completedTaskIds)
        {
            _notifiedTasksReminder.Remove(taskId);
            _notifiedTasksOverdue.Remove(taskId);
        }

        if (completedTaskIds.Count > 0)
        {
            logger.LogInformation("Cleaned up {Count} completed task(s) from tracking", completedTaskIds.Count);
        }
    }
}
