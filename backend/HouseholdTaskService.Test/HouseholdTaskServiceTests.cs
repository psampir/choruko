using System.Net;
using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Task;
using ChorukoAPI.Contracts.Responses.Task;
using ChorukoAPI.Data;
using ChorukoAPI.Mappers;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Service = ChorukoAPI.Services.Implementation.HouseholdTaskService;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using HouseholdTaskService.Test.Helpers;
using ChorukoAPI.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdTaskService.Test;

public class HouseholdTaskServiceTests
{
    private readonly DataContext _context;
    private readonly Mock<IMapper<HouseholdTask, HouseholdTaskResponse>> _mapperMock;
    private readonly Mock<IPushNotificationService> _pushMock;
    private readonly IHouseholdTaskService _service;

    public HouseholdTaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);
        _mapperMock = new Mock<IMapper<HouseholdTask, HouseholdTaskResponse>>();
        _pushMock = new Mock<IPushNotificationService>();

        InitializeNotificationHandler();

        _service = new Service(_context, _mapperMock.Object, _pushMock.Object);
    }

    private void InitializeNotificationHandler()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        var serviceScopeMock = new Mock<IServiceScope>();
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

        serviceProviderMock
            .Setup(x => x.GetService(typeof(IPushNotificationService)))
            .Returns(_pushMock.Object);

        serviceProviderMock
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactoryMock.Object);

        NotificationHandler.Initialize(serviceProviderMock.Object);
    }

    #region CreateHouseholdTaskAsync

    [Fact]
    public async Task CreateHouseholdTaskAsync_HouseholdNotFound_ReturnsNotFound()
    {
        var request = TestData.CreateTaskRequest();

        var result = await _service.CreateHouseholdTaskAsync("nonexistent-household", request, "user-1");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Contains("not found", result.Message);
    }

    [Fact]
    public async Task CreateHouseholdTaskAsync_InvalidActivityId_ReturnsBadRequest()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var request = TestData.CreateTaskRequest(activityId: "invalid-activity");

        _context.Users.Add(admin);
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var result = await _service.CreateHouseholdTaskAsync(household.Id, request, admin.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("invalid", result.Message.ToLower());
    }

    [Fact]
    public async Task CreateHouseholdTaskAsync_NoAssignees_CreatesUnassignedTask()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var activity = TestData.CreateActivity(household: household);
        var request = TestData.CreateTaskRequest(
            name: "Clean Kitchen",
            activityId: activity.Id,
            assigneeIds: new List<string>()
        );

        _context.Users.Add(admin);
        _context.Households.Add(household);
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        var result = await _service.CreateHouseholdTaskAsync(household.Id, request, admin.Id);

        Assert.True(result.IsSuccessful, $"Expected success but got: {result.Message}");
        Assert.Equal("CreateHouseholdTask", result.Result.Operation);

        var task = await _context.HouseholdTasks.FirstOrDefaultAsync();
        Assert.NotNull(task);
        Assert.Equal(ChorukoAPI.Models.TaskStatus.Unassigned, task.Status);
        Assert.Equal("Clean Kitchen", task.Name);
    }

    [Fact]
    public async Task CreateHouseholdTaskAsync_WithAssignees_CreatesAssignedTask()
    {
        var admin = TestData.CreateAdmin();
        var user = TestData.CreateUser("user-1", "user1@test.com");
        var household = TestData.CreateHousehold(admin: admin, participants: new List<User> { admin, user });
        var activity = TestData.CreateActivity(household: household);
        var request = TestData.CreateTaskRequest(
            name: "Wash Dishes",
            activityId: activity.Id,
            assigneeIds: new List<string> { user.Id }
        );

        _context.Users.AddRange(admin, user);
        _context.Households.Add(household);
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        var result = await _service.CreateHouseholdTaskAsync(household.Id, request, admin.Id);

        Assert.True(result.IsSuccessful);

        var task = await _context.HouseholdTasks.Include(t => t.Assignees).FirstOrDefaultAsync();
        Assert.NotNull(task);
        Assert.Equal(ChorukoAPI.Models.TaskStatus.Assigned, task.Status);
        Assert.Single(task.Assignees);
        Assert.Contains(task.Assignees, a => a.Id == user.Id);
    }

    [Fact]
    public async Task CreateHouseholdTaskAsync_DuplicateTaskName_ReturnsBadRequest()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var activity = TestData.CreateActivity(household: household);
        var existingTask = TestData.CreateHouseholdTask(
            name: "Duplicate Task",
            household: household,
            creator: admin,
            activity: activity
        );

        _context.Users.Add(admin);
        _context.Households.Add(household);
        _context.Activities.Add(activity);
        _context.HouseholdTasks.Add(existingTask);
        await _context.SaveChangesAsync();

        var request = TestData.CreateTaskRequest(name: "Duplicate Task", activityId: activity.Id);

        var result = await _service.CreateHouseholdTaskAsync(household.Id, request, admin.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("already exists", result.Message);
    }

    [Fact]
    public async Task CreateHouseholdTaskAsync_TooManyAssignees_ReturnsBadRequest()
    {
        var admin = TestData.CreateAdmin();
        var user1 = TestData.CreateUser("user-1");
        var user2 = TestData.CreateUser("user-2");
        var user3 = TestData.CreateUser("user-3");
        var household = TestData.CreateHousehold(
            admin: admin,
            participants: new List<User> { admin, user1, user2, user3 }
        );
        var activity = TestData.CreateActivity(household: household);
        var request = TestData.CreateTaskRequest(
            activityId: activity.Id,
            assigneeIds: new List<string> { user1.Id, user2.Id, user3.Id },
            participantsAmount: 2
        );

        _context.Users.AddRange(admin, user1, user2, user3);
        _context.Households.Add(household);
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        var result = await _service.CreateHouseholdTaskAsync(household.Id, request, admin.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Too many participants", result.Message);
    }

    #endregion

    #region EditHouseholdTaskAsync

    [Fact]
    public async Task EditHouseholdTaskAsync_TaskNotFound_ReturnsNotFound()
    {
        var request = TestData.EditTaskRequest();

        var result = await _service.EditHouseholdTaskAsync("nonexistent-task", request, "user-1");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task EditHouseholdTaskAsync_CompletedTask_ReturnsBadRequest()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var activity = TestData.CreateActivity(household: household);
        var task = TestData.CreateHouseholdTask(
            household: household,
            creator: admin,
            activity: activity,
            status: ChorukoAPI.Models.TaskStatus.Completed
        );

        _context.Users.Add(admin);
        _context.Households.Add(household);
        _context.Activities.Add(activity);
        _context.HouseholdTasks.Add(task);
        await _context.SaveChangesAsync();

        var request = TestData.EditTaskRequest(activityId: activity.Id);

        var result = await _service.EditHouseholdTaskAsync(task.Id, request, admin.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("completed", result.Message.ToLower());
    }

    [Fact]
    public async Task EditHouseholdTaskAsync_NotCreator_ReturnsBadRequest()
    {
        var admin = TestData.CreateAdmin();
        var creator = TestData.CreateUser("creator-1", "creator@test.com");
        var household = TestData.CreateHousehold(admin: admin, participants: new List<User> { admin, creator });
        var activity = TestData.CreateActivity(household: household);
        var task = TestData.CreateHouseholdTask(household: household, creator: creator, activity: activity);

        _context.Users.AddRange(admin, creator);
        _context.Households.Add(household);
        _context.Activities.Add(activity);
        _context.HouseholdTasks.Add(task);
        await _context.SaveChangesAsync();

        var request = TestData.EditTaskRequest(activityId: activity.Id);

        var result = await _service.EditHouseholdTaskAsync(task.Id, request, admin.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Only task creator", result.Message);
    }

    [Fact]
    public async Task EditHouseholdTaskAsync_ValidRequest_UpdatesTask()
    {
        var admin = TestData.CreateAdmin();
        var user = TestData.CreateUser("user-1");
        var household = TestData.CreateHousehold(admin: admin, participants: new List<User> { admin, user });
        var activity = TestData.CreateActivity(household: household);
        var task = TestData.CreateHouseholdTask(
            name: "Original Name",
            household: household,
            creator: admin,
            activity: activity
        );

        _context.Users.AddRange(admin, user);
        _context.Households.Add(household);
        _context.Activities.Add(activity);
        _context.HouseholdTasks.Add(task);
        await _context.SaveChangesAsync();

        var request = TestData.EditTaskRequest(
            name: "Updated Name",
            activityId: activity.Id,
            assigneeIds: new List<string> { user.Id }
        );

        var result = await _service.EditHouseholdTaskAsync(task.Id, request, admin.Id);

        var updated = await _context.HouseholdTasks.Include(t => t.Assignees).FirstOrDefaultAsync(t => t.Id == task.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal(ChorukoAPI.Models.TaskStatus.Assigned, updated.Status);
        Assert.Single(updated.Assignees);
    }

    #endregion

    #region GetAllHouseholdTaskAsync

    [Fact]
    public async Task GetAllHouseholdTaskAsync_ReturnsAllTasks()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var activity = TestData.CreateActivity(household: household);
        var task1 = TestData.CreateHouseholdTask(name: "Task 1", household: household, creator: admin, activity: activity);
        var task2 = TestData.CreateHouseholdTask(name: "Task 2", household: household, creator: admin, activity: activity);

        _context.Users.Add(admin);
        _context.Households.Add(household);
        _context.Activities.Add(activity);
        _context.HouseholdTasks.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map(It.IsAny<HouseholdTask>()))
            .ReturnsAsync((HouseholdTask t) => TestData.CreateTaskResponse(id: t.Id, name: t.Name));

        var result = await _service.GetAllHouseholdTaskAsync(household.Id);

        Assert.True(result.IsSuccessful);
        Assert.Equal(2, result.Result.Count);
    }

    #endregion

    #region GetHouseholdTaskByIdAsync

    [Fact]
    public async Task GetHouseholdTaskByIdAsync_TaskNotFound_ReturnsNotFound()
    {
        var result = await _service.GetHouseholdTaskByIdAsync("nonexistent-task");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task GetHouseholdTaskByIdAsync_ValidTask_ReturnsTask()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var activity = TestData.CreateActivity(household: household);
        var task = TestData.CreateHouseholdTask(household: household, creator: admin, activity: activity);

        _context.Users.Add(admin);
        _context.Households.Add(household);
        _context.Activities.Add(activity);
        _context.HouseholdTasks.Add(task);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map(It.IsAny<HouseholdTask>()))
            .ReturnsAsync(TestData.CreateTaskResponse(id: task.Id, name: task.Name));

        var result = await _service.GetHouseholdTaskByIdAsync(task.Id);

        Assert.True(result.IsSuccessful);
        Assert.Equal(task.Id, result.Result.Id);
    }

    #endregion

    #region AssignUserToTask

    [Fact]
    public async Task AssignUserToTask_UserNotFound_ReturnsNotFound()
    {
        var result = await _service.AssignUserToTask("task-1", "nonexistent-user");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Contains("User not found", result.Message);
    }

    [Fact]
    public async Task AssignUserToTask_TaskNotFound_ReturnsNotFound()
    {
        var user = TestData.CreateUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _service.AssignUserToTask("nonexistent-task", user.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Contains("Task not found", result.Message);
    }

    [Fact]
    public async Task AssignUserToTask_CompletedTask_ReturnsBadRequest()
    {
        var admin = TestData.CreateAdmin();
        var user = TestData.CreateUser("user-1");
        var household = TestData.CreateHousehold(admin: admin);
        var activity = TestData.CreateActivity(household: household);
        var task = TestData.CreateHouseholdTask(
            household: household,
            creator: admin,
            activity: activity,
            status: ChorukoAPI.Models.TaskStatus.Completed
        );

        _context.Users.AddRange(admin, user);
        _context.Households.Add(household);
        _context.Activities.Add(activity);
        _context.HouseholdTasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _service.AssignUserToTask(task.Id, user.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task AssignUserToTask_ValidRequest_AssignsUser()
    {
        var admin = TestData.CreateAdmin();
        var user = TestData.CreateUser("user-1");
        var household = TestData.CreateHousehold(admin: admin);
        var activity = TestData.CreateActivity(household: household);
        var task = TestData.CreateHouseholdTask(household: household, creator: admin, activity: activity);

        _context.Users.AddRange(admin, user);
        _context.Households.Add(household);
        _context.Activities.Add(activity);
        _context.HouseholdTasks.Add(task);
        await _context.SaveChangesAsync();

        _pushMock.Setup(p => p.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ServiceResponseFactory.Success(new OperationResultDto(), "OK"));

        var result = await _service.AssignUserToTask(task.Id, user.Id);

        Assert.True(result.IsSuccessful);

        var updated = await _context.HouseholdTasks.Include(t => t.Assignees).FirstOrDefaultAsync(t => t.Id == task.Id);
        Assert.NotNull(updated);
        Assert.Equal(ChorukoAPI.Models.TaskStatus.Assigned, updated.Status);
        Assert.Contains(updated.Assignees, a => a.Id == user.Id);
    }

    #endregion

    #region UnAssignUserFromTask

    [Fact]
    public async Task UnAssignUserFromTask_UserNotFound_ReturnsNotFound()
    {
        var result = await _service.UnAssignUserFromTask("task-1", "nonexistent-user");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task UnAssignUserFromTask_ValidRequest_UnassignsUser()
    {
        var admin = TestData.CreateAdmin();
        var user = TestData.CreateUser("user-1");
        var household = TestData.CreateHousehold(admin: admin);
        var activity = TestData.CreateActivity(household: household);
        var task = TestData.CreateHouseholdTask(
            household: household,
            creator: admin,
            activity: activity,
            assignees: new List<User> { user },
            status: ChorukoAPI.Models.TaskStatus.Assigned
        );

        _context.Users.AddRange(admin, user);
        _context.Households.Add(household);
        _context.Activities.Add(activity);
        _context.HouseholdTasks.Add(task);
        await _context.SaveChangesAsync();

        _pushMock.Setup(p => p.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ServiceResponseFactory.Success(new OperationResultDto(), "OK"));

        var result = await _service.UnAssignUserFromTask(task.Id, user.Id);

        Assert.True(result.IsSuccessful);

        var updated = await _context.HouseholdTasks.Include(t => t.Assignees).FirstOrDefaultAsync(t => t.Id == task.Id);
        Assert.NotNull(updated);
        Assert.Equal(ChorukoAPI.Models.TaskStatus.Unassigned, updated.Status);
        Assert.Empty(updated.Assignees);
    }

    #endregion

    #region MarkTaskAsCompleted

    [Fact]
    public async Task MarkTaskAsCompleted_TaskNotFound_ReturnsNotFound()
    {
        var result = await _service.MarkTaskAsCompleted("nonexistent-task", "user-1");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task MarkTaskAsCompleted_ValidRequest_UpdatesStatusAndPoints()
    {
        var admin = TestData.CreateAdmin();
        var user = TestData.CreateUser("user-1");
        var household = TestData.CreateHousehold(admin: admin);
        var activity = TestData.CreateActivity(household: household, pointsAwarded: 50);
        var task = TestData.CreateHouseholdTask(
            household: household,
            creator: admin,
            activity: activity,
            assignees: new List<User> { user },
            status: ChorukoAPI.Models.TaskStatus.Assigned
        );

        _context.Users.AddRange(admin, user);
        _context.Households.Add(household);
        _context.Activities.Add(activity);
        _context.HouseholdTasks.Add(task);
        await _context.SaveChangesAsync();

        _pushMock.Setup(p => p.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ServiceResponseFactory.Success(new OperationResultDto(), "OK"));

        var result = await _service.MarkTaskAsCompleted(task.Id, admin.Id);

        Assert.True(result.IsSuccessful);

        var updated = await _context.HouseholdTasks.FirstOrDefaultAsync(t => t.Id == task.Id);
        var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(updated);
        Assert.NotNull(updatedUser);
        Assert.Equal(ChorukoAPI.Models.TaskStatus.Completed, updated.Status);
        Assert.Equal(50, updatedUser.PointsCurrentMonth);
        Assert.Equal(50, updatedUser.PointsCurrentYear);
        Assert.Equal(50, updatedUser.PointsAllTime);
    }

    #endregion

    #region UnMarkTaskAsCompleted

    [Fact]
    public async Task UnMarkTaskAsCompleted_TaskNotCompleted_ReturnsBadRequest()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var activity = TestData.CreateActivity(household: household);
        var task = TestData.CreateHouseholdTask(
            household: household,
            creator: admin,
            activity: activity,
            status: ChorukoAPI.Models.TaskStatus.Assigned
        );

        _context.Users.Add(admin);
        _context.Households.Add(household);
        _context.Activities.Add(activity);
        _context.HouseholdTasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _service.UnMarkTaskAsCompleted(task.Id, admin.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("not currently marked as completed", result.Message);
    }

    [Fact]
    public async Task UnMarkTaskAsCompleted_ValidRequest_RevertsStatusAndPoints()
    {
        var admin = TestData.CreateAdmin();
        var user = TestData.CreateUser("user-1");
        user.PointsCurrentMonth = 50;
        user.PointsCurrentYear = 50;
        user.PointsAllTime = 50;

        var household = TestData.CreateHousehold(admin: admin);
        var activity = TestData.CreateActivity(household: household, pointsAwarded: 50);
        var task = TestData.CreateHouseholdTask(
            household: household,
            creator: admin,
            activity: activity,
            assignees: new List<User> { user },
            status: ChorukoAPI.Models.TaskStatus.Completed
        );

        _context.Users.AddRange(admin, user);
        _context.Households.Add(household);
        _context.Activities.Add(activity);
        _context.HouseholdTasks.Add(task);
        await _context.SaveChangesAsync();

        _pushMock.Setup(p => p.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ServiceResponseFactory.Success(new OperationResultDto(), "OK"));

        var result = await _service.UnMarkTaskAsCompleted(task.Id, admin.Id);

        Assert.True(result.IsSuccessful);

        var updated = await _context.HouseholdTasks.Include(t => t.Assignees).FirstOrDefaultAsync(t => t.Id == task.Id);
        var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(updated);
        Assert.NotNull(updatedUser);
        Assert.Equal(ChorukoAPI.Models.TaskStatus.Assigned, updated.Status);
        Assert.Equal(0, updatedUser.PointsCurrentMonth);
        Assert.Equal(0, updatedUser.PointsCurrentYear);
        Assert.Equal(0, updatedUser.PointsAllTime);
    }

    #endregion

    #region DeleteHouseholdTask

    [Fact]
    public async Task DeleteHouseholdTask_TaskNotFound_ReturnsNotFound()
    {
        var result = await _service.DeleteHouseholdTask("nonexistent-task");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task DeleteHouseholdTask_CompletedTask_ReturnsBadRequest()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var activity = TestData.CreateActivity(household: household);
        var task = TestData.CreateHouseholdTask(
            household: household,
            creator: admin,
            activity: activity,
            status: ChorukoAPI.Models.TaskStatus.Completed
        );

        _context.Users.Add(admin);
        _context.Households.Add(household);
        _context.Activities.Add(activity);
        _context.HouseholdTasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteHouseholdTask(task.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Cannot delete a completed task", result.Message);
    }

    [Fact]
    public async Task DeleteHouseholdTask_ValidRequest_DeletesTask()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var activity = TestData.CreateActivity(household: household);
        var task = TestData.CreateHouseholdTask(household: household, creator: admin, activity: activity);

        _context.Users.Add(admin);
        _context.Households.Add(household);
        _context.Activities.Add(activity);
        _context.HouseholdTasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteHouseholdTask(task.Id);

        Assert.True(result.IsSuccessful);

        var deleted = await _context.HouseholdTasks.FirstOrDefaultAsync(t => t.Id == task.Id);
        Assert.Null(deleted);
    }

    #endregion
}
