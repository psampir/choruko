using ChorukoAPI.Contracts.Requests.Task;
using ChorukoAPI.Contracts.Responses.Activity;
using ChorukoAPI.Contracts.Responses.Task;
using ChorukoAPI.Contracts.Responses.User;
using ChorukoAPI.Models;
using System.Diagnostics;

namespace HouseholdTaskService.Test.Helpers;

public static class TestData
{
    // Users
    public static User CreateUser(
        string? id = null,
        string? email = null,
        string? firstName = null,
        string? lastName = null,
        string? householdId = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Email = email ?? "user@test.com",
            FirstName = firstName ?? "John",
            LastName = lastName ?? "Doe",
            HouseholdId = householdId,
            PointsCurrentMonth = 0,
            PointsCurrentYear = 0,
            PointsAllTime = 0
        };
    }

    public static User CreateAdmin(string? id = null)
    {
        return CreateUser(
            id: id ?? "admin-1",
            email: "admin@test.com",
            firstName: "Admin",
            lastName: "User"
        );
    }

    // Households
    public static Household CreateHousehold(
        string? id = null,
        string? name = null,
        User? admin = null,
        string? currencyCode = null,
        List<User>? participants = null)
    {
        var adminUser = admin ?? CreateAdmin();
        return new Household
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Name = name ?? "Test Household",
            AdminId = adminUser.Id,
            Admin = adminUser,
            CurrencyCode = currencyCode ?? "USD",
            Participants = participants ?? new List<User> { adminUser },
            Activities = new List<ChorukoAPI.Models.Activity>(),
            Tasks = new List<HouseholdTask>()
        };
    }

    // Activities
    public static ChorukoAPI.Models.Activity CreateActivity(
        string? id = null,
        string? name = null,
        Household? household = null,
        int pointsAwarded = 10)
    {
        var householdData = household ?? CreateHousehold();
        return new ChorukoAPI.Models.Activity
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Name = name ?? "Test Activity",
            HouseholdId = householdData.Id,
            Household = householdData,
            PointsAwarded = pointsAwarded
        };
    }

    // HouseholdTasks
    public static HouseholdTask CreateHouseholdTask(
        string? id = null,
        string? name = null,
        string? description = null,
        Household? household = null,
        User? creator = null,
        ChorukoAPI.Models.Activity? activity = null,
        List<User>? assignees = null,
        ChorukoAPI.Models.TaskStatus? status = null,
        DateTime? deadline = null,
        int participantsAmount = 5)
    {
        var householdData = household ?? CreateHousehold();
        var creatorUser = creator ?? CreateUser();
        var activityData = activity ?? CreateActivity(household: householdData);

        return new HouseholdTask
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Name = name ?? "Test Task",
            Description = description ?? "Test Description",
            HouseholdId = householdData.Id,
            Household = householdData,
            CreatorId = creatorUser.Id,
            Creator = creatorUser,
            ActivityId = activityData.Id,
            Activity = activityData,
            Assignees = assignees ?? new List<User>(),
            Status = status ?? ChorukoAPI.Models.TaskStatus.Unassigned,
            DeadLine = deadline ?? DateTime.UtcNow.AddDays(7),
            CreationDate = DateTime.UtcNow,
            ParticipantsAmount = participantsAmount
        };
    }

    // Request DTOs
    public static CreateHouseholdTaskRequest CreateTaskRequest(
        string? name = null,
        string? description = null,
        string? activityId = null,
        List<string>? assigneeIds = null,
        DateTime? deadline = null,
        int participantsAmount = 5)
    {
        return new CreateHouseholdTaskRequest
        {
            Name = name ?? "Test Task",
            Description = description ?? "Test Description",
            ActivityId = activityId,
            AssigneeIds = assigneeIds ?? new List<string>(),
            DeadLine = deadline ?? DateTime.UtcNow.AddDays(7),
            ParticipantsAmount = participantsAmount
        };
    }

    public static EditHouseholdTaskRequest EditTaskRequest(
        string? name = null,
        string? description = null,
        string? activityId = null,
        List<string>? assigneeIds = null,
        DateTime? deadline = null,
        int participantsAmount = 5)
    {
        return new EditHouseholdTaskRequest
        {
            Name = name ?? "Edited Task",
            Description = description ?? "Edited Description",
            ActivityId = activityId,
            AssigneeIds = assigneeIds ?? new List<string>(),
            DeadLine = deadline ?? DateTime.UtcNow.AddDays(10),
            ParticipantsAmount = participantsAmount
        };
    }

    // Response DTOs
    public static HouseholdTaskResponse CreateTaskResponse(
    string? id = null,
    string? householdId = null,
    string? activityId = null,
    ActivityResponse? activity = null,
    string? creatorId = null,
    UserInfoResponse? creator = null,
    string? name = null,
    string? description = null,
    DateTime? creationDate = null,
    DateTime? deadline = null,
    ChorukoAPI.Models.TaskStatus? status = null,
    int participantsAmount = 5,
    List<UserInfoResponse>? assignees = null)
    {
        return new HouseholdTaskResponse
        {
            Id = id ?? Guid.NewGuid().ToString(),
            HouseholdId = householdId ?? Guid.NewGuid().ToString(),
            ActivityId = activityId ?? Guid.NewGuid().ToString(),
            Activity = activity ?? CreateActivityResponse(),
            CreatorId = creatorId ?? Guid.NewGuid().ToString(),
            Creator = creator ?? CreateUserInfoResponse(),
            Name = name ?? "Test Task",
            Description = description ?? "Test Description",
            CreationDate = creationDate ?? DateTime.UtcNow,
            DeadLine = deadline ?? DateTime.UtcNow.AddDays(7),
            Status = status ?? ChorukoAPI.Models.TaskStatus.Unassigned,
            ParticipantsAmount = participantsAmount,
            Assignees = assignees ?? new List<UserInfoResponse>()
        };
    }

    // Helper methods for nested response objects
    public static ActivityResponse CreateActivityResponse(
        string? id = null,
        string? name = null,
        int pointsAwarded = 10)
    {
        return new ActivityResponse
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Name = name ?? "Test Activity",
            PointsAwarded = pointsAwarded
        };
    }

    public static UserInfoResponse CreateUserInfoResponse(
    string? id = null,
    string? firstName = null,
    string? lastName = null,
    string? email = null,
    string? householdId = null,
    string? profilePicturePath = null,
    int pointsCurrentMonth = 0,
    int pointsCurrentYear = 0,
    int pointsAllTime = 0)
    {
        return new UserInfoResponse(
            Id: id ?? Guid.NewGuid().ToString(),
            FirstName: firstName ?? "John",
            LastName: lastName ?? "Doe",
            Email: email ?? "user@test.com",
            HouseholdId: householdId,
            ProfilePicturePath: profilePicturePath,
            PointsCurrentMonth: pointsCurrentMonth,
            PointsCurrentYear: pointsCurrentYear,
            PointsAllTime: pointsAllTime
        );
    }
}
