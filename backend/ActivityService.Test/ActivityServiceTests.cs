using System.Net;
using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Activity;
using ChorukoAPI.Contracts.Responses.Activity;
using ChorukoAPI.Data;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Service = ChorukoAPI.Services.Implementation.ActivityService;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ActivityService.Test.Helpers;

namespace ActivityService.Test;

public class ActivityServiceTests : IDisposable
{
    private readonly DataContext _context;
    private readonly IActivityService _service;

    public ActivityServiceTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);
        _service = new Service(_context);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    #region CreateActivityAsync

    [Fact]
    public async Task CreateActivityAsync_HouseholdNotFound_ReturnsNotFound()
    {
        var request = TestData.CreateActivityRequest();

        var result = await _service.CreateActivityAsync("nonexistent-household", request);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Contains("Household not found", result.Message);
    }

    [Fact]
    public async Task CreateActivityAsync_DuplicateActivityName_ReturnsBadRequest()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var existingActivity = TestData.CreateActivity(
            name: "Wash Dishes",
            household: household
        );

        _context.Users.Add(admin);
        _context.Households.Add(household);
        _context.Activities.Add(existingActivity);
        await _context.SaveChangesAsync();

        var request = TestData.CreateActivityRequest(name: "Wash Dishes");

        var result = await _service.CreateActivityAsync(household.Id, request);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("already exist", result.Message);
    }

    [Fact]
    public async Task CreateActivityAsync_ValidRequest_CreatesActivity()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);

        _context.Users.Add(admin);
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var request = TestData.CreateActivityRequest(
            name: "Clean Kitchen",
            pointsAwarded: 15
        );

        var result = await _service.CreateActivityAsync(household.Id, request);

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Result);
        Assert.Equal("CreateActivity", result.Result.Operation);

        var activity = await _context.Activities.FirstOrDefaultAsync(a => a.Name == "Clean Kitchen");
        Assert.NotNull(activity);
        Assert.Equal(15, activity.PointsAwarded);
        Assert.Equal(household.Id, activity.HouseholdId);
    }

    #endregion

    #region EditActivityAsync

    [Fact]
    public async Task EditActivityAsync_ActivityNotFound_ReturnsNotFound()
    {
        var request = TestData.EditActivityRequest();

        var result = await _service.EditActivityAsync("nonexistent-activity", request);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Contains("Activity not found", result.Message);
    }

    [Fact]
    public async Task EditActivityAsync_DuplicateNameInSameHousehold_ReturnsBadRequest()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var activity1 = TestData.CreateActivity(
            id: "act-1",
            name: "Activity 1",
            household: household
        );
        var activity2 = TestData.CreateActivity(
            id: "act-2",
            name: "Activity 2",
            household: household
        );

        _context.Users.Add(admin);
        _context.Households.Add(household);
        _context.Activities.AddRange(activity1, activity2);
        await _context.SaveChangesAsync();

        var request = TestData.EditActivityRequest(name: "Activity 1");

        var result = await _service.EditActivityAsync(activity2.Id, request);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("already exist", result.Message);
    }

    [Fact]
    public async Task EditActivityAsync_ValidRequest_UpdatesActivity()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var activity = TestData.CreateActivity(
            name: "Original Name",
            household: household,
            pointsAwarded: 10
        );

        _context.Users.Add(admin);
        _context.Households.Add(household);
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        var request = TestData.EditActivityRequest(
            name: "Updated Name",
            pointsAwarded: 25
        );

        var result = await _service.EditActivityAsync(activity.Id, request);

        Assert.True(result.IsSuccessful);
        Assert.Equal("EditActivity", result.Result.Operation);

        var updated = await _context.Activities.FindAsync(activity.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal(25, updated.PointsAwarded);
    }

    [Fact]
    public async Task EditActivityAsync_SameNameForSameActivity_Succeeds()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var activity = TestData.CreateActivity(
            name: "Same Name",
            household: household,
            pointsAwarded: 10
        );

        _context.Users.Add(admin);
        _context.Households.Add(household);
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        var request = TestData.EditActivityRequest(
            name: "Same Name",
            pointsAwarded: 20
        );

        var result = await _service.EditActivityAsync(activity.Id, request);

        Assert.True(result.IsSuccessful);

        var updated = await _context.Activities.FindAsync(activity.Id);
        Assert.NotNull(updated);
        Assert.Equal("Same Name", updated.Name);
        Assert.Equal(20, updated.PointsAwarded);
    }

    #endregion

    #region GetActivitiesAsync

    [Fact]
    public async Task GetActivitiesAsync_NoActivities_ReturnsEmptyList()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);

        _context.Users.Add(admin);
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var result = await _service.GetActivitiesAsync(household.Id);

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Result);
        Assert.Empty(result.Result);
    }

    [Fact]
    public async Task GetActivitiesAsync_MultipleActivities_ReturnsAll()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var activity1 = TestData.CreateActivity(name: "Activity 1", household: household, pointsAwarded: 10);
        var activity2 = TestData.CreateActivity(name: "Activity 2", household: household, pointsAwarded: 15);
        var activity3 = TestData.CreateActivity(name: "Activity 3", household: household, pointsAwarded: 20);

        _context.Users.Add(admin);
        _context.Households.Add(household);
        _context.Activities.AddRange(activity1, activity2, activity3);
        await _context.SaveChangesAsync();

        var result = await _service.GetActivitiesAsync(household.Id);

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Result);
        Assert.Equal(3, result.Result.Count);
        Assert.Contains(result.Result, a => a.Name == "Activity 1");
        Assert.Contains(result.Result, a => a.Name == "Activity 2");
        Assert.Contains(result.Result, a => a.Name == "Activity 3");
    }

    [Fact]
    public async Task GetActivitiesAsync_OnlyReturnsActivitiesForSpecificHousehold()
    {
        var admin1 = TestData.CreateAdmin("admin-1");
        var admin2 = TestData.CreateAdmin("admin-2");
        var household1 = TestData.CreateHousehold("h-1", admin: admin1);
        var household2 = TestData.CreateHousehold("h-2", admin: admin2);
        var activity1 = TestData.CreateActivity(name: "H1 Activity", household: household1);
        var activity2 = TestData.CreateActivity(name: "H2 Activity", household: household2);

        _context.Users.AddRange(admin1, admin2);
        _context.Households.AddRange(household1, household2);
        _context.Activities.AddRange(activity1, activity2);
        await _context.SaveChangesAsync();

        var result = await _service.GetActivitiesAsync(household1.Id);

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Result);
        Assert.Equal("H1 Activity", result.Result.First().Name);
    }

    #endregion

    #region GetActivityByIdAsync

    [Fact]
    public async Task GetActivityByIdAsync_ActivityNotFound_ReturnsNotFound()
    {
        var result = await _service.GetActivityByIdAsync("nonexistent-activity");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Contains("No activity found", result.Message);
    }

    [Fact]
    public async Task GetActivityByIdAsync_ValidActivity_ReturnsActivity()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var activity = TestData.CreateActivity(
            id: "act-123",
            name: "Test Activity",
            household: household,
            pointsAwarded: 25
        );

        _context.Users.Add(admin);
        _context.Households.Add(household);
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        var result = await _service.GetActivityByIdAsync(activity.Id);

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Result);
        Assert.Equal("act-123", result.Result.Id);
        Assert.Equal("Test Activity", result.Result.Name);
        Assert.Equal(25, result.Result.PointsAwarded);
    }

    #endregion

    #region DeleteActivityAsync

    [Fact]
    public async Task DeleteActivityAsync_ActivityNotFound_ReturnsNotFound()
    {
        var result = await _service.DeleteActivityAsync("nonexistent-activity");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Contains("No activity found", result.Message);
    }

    [Fact]
    public async Task DeleteActivityAsync_ValidActivity_DeletesActivity()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var activity = TestData.CreateActivity(
            name: "Activity to Delete",
            household: household
        );

        _context.Users.Add(admin);
        _context.Households.Add(household);
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteActivityAsync(activity.Id);

        Assert.True(result.IsSuccessful);
        Assert.Equal("DeleteActivity", result.Result.Operation);

        var deleted = await _context.Activities.FindAsync(activity.Id);
        Assert.Null(deleted);
    }

    #endregion
}
