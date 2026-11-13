using System.Net;
using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Household;
using ChorukoAPI.Contracts.Responses.Household;
using ChorukoAPI.Data;
using ChorukoAPI.Extensions;
using ChorukoAPI.Mappers;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Service = ChorukoAPI.Services.Implementation.HouseholdService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using HouseholdService.Test.Helpers;

namespace HouseholdService.Test;

public class HouseholdServiceTests : IDisposable
{
    private readonly DataContext _context;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<SignInManager<User>> _signInManagerMock;
    private readonly Mock<IImageProcessor> _imageProcessorMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IMapper<Household, HouseholdInfoResponse>> _mapperMock;
    private readonly Mock<IPushNotificationService> _pushMock;
    private readonly IHouseholdService _service;

    public HouseholdServiceTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);
        _userManagerMock = GetMockUserManager();
        _signInManagerMock = GetMockSignInManager(_userManagerMock);
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _mapperMock = new Mock<IMapper<Household, HouseholdInfoResponse>>();
        _pushMock = new Mock<IPushNotificationService>();
        _imageProcessorMock = new Mock<IImageProcessor>();
        _imageProcessorMock
            .Setup(x => x.ProcessAsync(It.IsAny<IFormFile?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResponseFactory.Success<string>(null, "No profile picture provided.", HttpStatusCode.OK));

        _service = new Service(
            _context,
            _imageProcessorMock.Object,
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _httpContextAccessorMock.Object,
            _mapperMock.Object,
            _pushMock.Object
        );
    }

    private Mock<UserManager<User>> GetMockUserManager()
    {
        var store = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
    }

    private Mock<SignInManager<User>> GetMockSignInManager(Mock<UserManager<User>> userManagerMock)
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
        return new Mock<SignInManager<User>>(
            userManagerMock.Object,
            contextAccessor.Object,
            claimsFactory.Object,
            null, null, null, null
        );
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    #region GetHouseholdInfoAsync

    [Fact]
    public async Task GetHouseholdInfoAsync_UserNotFound_ReturnsNotFound()
    {
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync((User)null);

        var result = await _service.GetHouseholdInfoAsync();

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Contains("User not found", result.Message);
    }

    [Fact]
    public async Task GetHouseholdInfoAsync_UserNotInHousehold_ReturnsBadRequest()
    {
        var user = TestData.CreateUser();

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var result = await _service.GetHouseholdInfoAsync();

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("not part of any household", result.Message);
    }

    [Fact]
    public async Task GetHouseholdInfoAsync_ValidUser_ReturnsHouseholdInfo()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        admin.HouseholdId = household.Id;

        _context.Users.Add(admin);
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(admin);

        _mapperMock.Setup(m => m.Map(It.IsAny<Household>()))
            .ReturnsAsync(TestData.CreateHouseholdInfoResponse(id: household.Id, name: household.Name));

        var result = await _service.GetHouseholdInfoAsync();

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Result);
        Assert.Equal(household.Id, result.Result.Id);
    }

    #endregion

    #region CreateHouseholdAsync

    [Fact]
    public async Task CreateHouseholdAsync_UserNotFound_ReturnsNotFound()
    {
        var request = TestData.CreateHouseholdRequest();

        _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);

        var result = await _service.CreateHouseholdAsync(request, "user-1");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Contains("User not found", result.Message);
    }

    [Fact]
    public async Task CreateHouseholdAsync_UserAlreadyInHousehold_ReturnsConflict()
    {
        var user = TestData.CreateUser(householdId: "existing-household");
        var request = TestData.CreateHouseholdRequest();

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(u => u.FindByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await _service.CreateHouseholdAsync(request, user.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        Assert.Contains("already belongs to a household", result.Message);
    }

    [Fact]
    public async Task CreateHouseholdAsync_UserNotInNoneRole_ReturnsForbidden()
    {
        var user = TestData.CreateUser();
        var request = TestData.CreateHouseholdRequest();

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(u => u.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Participant" });

        var result = await _service.CreateHouseholdAsync(request, user.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        Assert.Contains("'None' role", result.Message);
    }

    [Fact]
    public async Task CreateHouseholdAsync_ValidRequest_CreatesHousehold()
    {
        var user = TestData.CreateUser();
        var request = TestData.CreateHouseholdRequest(name: "My Household", currencyCode: "USD");

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(u => u.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "None" });
        _userManagerMock.Setup(u => u.RemoveFromRoleAsync(user, "None"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.AddToRoleAsync(user, "HouseholdAdmin"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        _signInManagerMock.Setup(s => s.RefreshSignInAsync(user))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateHouseholdAsync(request, user.Id);

        Assert.True(result.IsSuccessful);
        Assert.Equal("CreateHousehold", result.Result.Operation);

        var household = await _context.Households.FirstOrDefaultAsync();
        Assert.NotNull(household);
        Assert.Equal("My Household", household.Name);
        Assert.Equal("USD", household.CurrencyCode);
        Assert.Equal(user.Id, household.AdminId);

        var activities = await _context.Activities.Where(a => a.HouseholdId == household.Id).ToListAsync();
        Assert.Equal(5, activities.Count);
    }

    #endregion

    #region DeleteUserFromHouseholdAsync

    [Fact]
    public async Task DeleteUserFromHouseholdAsync_UserToDeleteNotFound_ReturnsNotFound()
    {
        _userManagerMock.Setup(u => u.FindByIdAsync("user-to-delete"))
            .ReturnsAsync((User)null);

        var result = await _service.DeleteUserFromHouseholdAsync("user-to-delete", "current-user");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task DeleteUserFromHouseholdAsync_CannotRemoveLastAdmin_ReturnsForbidden()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        admin.HouseholdId = household.Id;

        _context.Users.Add(admin);
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(u => u.FindByIdAsync(admin.Id)).ReturnsAsync(admin);
        _userManagerMock.Setup(u => u.GetRolesAsync(admin))
            .ReturnsAsync(new List<string> { "HouseholdAdmin" });

        var result = await _service.DeleteUserFromHouseholdAsync(admin.Id, admin.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        Assert.Contains("at least one admin", result.Message);
    }

    [Fact]
    public async Task DeleteUserFromHouseholdAsync_ValidRequest_RemovesUser()
    {
        var admin = TestData.CreateAdmin();
        var participant = TestData.CreateUser("participant-1", householdId: "h-1");
        var household = TestData.CreateHousehold("h-1", admin: admin, participants: new List<User> { admin, participant });
        admin.HouseholdId = household.Id;
        participant.HouseholdId = household.Id;

        _context.Users.AddRange(admin, participant);
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(u => u.FindByIdAsync(participant.Id)).ReturnsAsync(participant);
        _userManagerMock.Setup(u => u.FindByIdAsync(admin.Id)).ReturnsAsync(admin);
        _userManagerMock.Setup(u => u.GetRolesAsync(participant))
            .ReturnsAsync(new List<string> { "Participant" });
        _userManagerMock.Setup(u => u.RemoveFromRolesAsync(participant, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.AddToRoleAsync(participant, "None"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _service.DeleteUserFromHouseholdAsync(participant.Id, admin.Id);

        Assert.True(result.IsSuccessful);

        var updated = await _context.Users.FindAsync(participant.Id);
        Assert.Null(updated.HouseholdId);
    }

    #endregion

    #region DeleteHouseholdAsync

    [Fact]
    public async Task DeleteHouseholdAsync_UserNotAdmin_ReturnsForbidden()
    {
        var user = TestData.CreateUser();

        _userManagerMock.Setup(u => u.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Participant" });

        var result = await _service.DeleteHouseholdAsync(user.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        Assert.Contains("Only household admins", result.Message);
    }

    [Fact]
    public async Task DeleteHouseholdAsync_ValidRequest_DeletesHousehold()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        admin.HouseholdId = household.Id;

        _context.Users.Add(admin);
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(u => u.FindByIdAsync(admin.Id)).ReturnsAsync(admin);
        _userManagerMock.Setup(u => u.GetRolesAsync(admin))
            .ReturnsAsync(new List<string> { "HouseholdAdmin" });
        _userManagerMock.Setup(u => u.RemoveFromRolesAsync(admin, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.AddToRoleAsync(admin, "None"))
            .ReturnsAsync(IdentityResult.Success);
        _signInManagerMock.Setup(s => s.RefreshSignInAsync(admin))
            .Returns(Task.CompletedTask);

        var result = await _service.DeleteHouseholdAsync(admin.Id);

        Assert.True(result.IsSuccessful);

        var deleted = await _context.Households.FindAsync(household.Id);
        Assert.Null(deleted);
    }

    #endregion

    #region RenameHouseholdAsync

    [Fact]
    public async Task RenameHouseholdAsync_UserNotAdmin_ReturnsForbidden()
    {
        var user = TestData.CreateUser();

        _userManagerMock.Setup(u => u.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Participant" });

        var result = await _service.RenameHouseholdAsync(user.Id, "New Name");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task RenameHouseholdAsync_ValidRequest_RenamesHousehold()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold("h-1", "Old Name", admin: admin);
        admin.HouseholdId = household.Id;

        _context.Users.Add(admin);
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(u => u.FindByIdAsync(admin.Id)).ReturnsAsync(admin);
        _userManagerMock.Setup(u => u.GetRolesAsync(admin))
            .ReturnsAsync(new List<string> { "HouseholdAdmin" });

        var result = await _service.RenameHouseholdAsync(admin.Id, "New Name");

        Assert.True(result.IsSuccessful);

        var updated = await _context.Households.FindAsync(household.Id);
        Assert.Equal("New Name", updated.Name);
    }

    #endregion

    #region LeaveHouseholdAsync

    [Fact]
    public async Task LeaveHouseholdAsync_UserNotInHousehold_ReturnsBadRequest()
    {
        var user = TestData.CreateUser();

        _userManagerMock.Setup(u => u.FindByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await _service.LeaveHouseholdAsync(user.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task LeaveHouseholdAsync_LastAdmin_ReturnsForbidden()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        admin.HouseholdId = household.Id;

        _context.Users.Add(admin);
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(u => u.FindByIdAsync(admin.Id)).ReturnsAsync(admin);
        _userManagerMock.Setup(u => u.GetRolesAsync(admin))
            .ReturnsAsync(new List<string> { "HouseholdAdmin" });

        var result = await _service.LeaveHouseholdAsync(admin.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task LeaveHouseholdAsync_ValidRequest_UserLeavesHousehold()
    {
        var admin = TestData.CreateAdmin();
        var participant = TestData.CreateUser("participant-1");
        var household = TestData.CreateHousehold(admin: admin, participants: new List<User> { admin, participant });
        participant.HouseholdId = household.Id;

        _context.Users.AddRange(admin, participant);
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(u => u.FindByIdAsync(participant.Id)).ReturnsAsync(participant);
        _userManagerMock.Setup(u => u.GetRolesAsync(participant))
            .ReturnsAsync(new List<string> { "Participant" });
        _userManagerMock.Setup(u => u.GetRolesAsync(admin))
            .ReturnsAsync(new List<string> { "HouseholdAdmin" });
        _userManagerMock.Setup(u => u.RemoveFromRolesAsync(participant, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.AddToRoleAsync(participant, "None"))
            .ReturnsAsync(IdentityResult.Success);
        _signInManagerMock.Setup(s => s.RefreshSignInAsync(participant))
            .Returns(Task.CompletedTask);

        var result = await _service.LeaveHouseholdAsync(participant.Id);

        Assert.True(result.IsSuccessful);

        var updated = await _context.Users.FindAsync(participant.Id);
        Assert.Null(updated.HouseholdId);
    }

    #endregion

    #region SetHouseholdAdminAsync

    [Fact]
    public async Task SetHouseholdAdminAsync_UsersNotFound_ReturnsNotFound()
    {
        _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);

        var result = await _service.SetHouseholdAdminAsync("user-1", "user-2");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task SetHouseholdAdminAsync_UserNotInSameHousehold_ReturnsBadRequest()
    {
        var admin = TestData.CreateAdmin(householdId: "h-1");
        var user = TestData.CreateUser(householdId: "h-2");

        _userManagerMock.Setup(u => u.FindByIdAsync(admin.Id)).ReturnsAsync(admin);
        _userManagerMock.Setup(u => u.FindByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await _service.SetHouseholdAdminAsync(admin.Id, user.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("same household", result.Message);
    }

    [Fact]
    public async Task SetHouseholdAdminAsync_ValidRequest_SwapsRoles()
    {
        var admin = TestData.CreateAdmin();
        var participant = TestData.CreateUser("participant-1");
        var household = TestData.CreateHousehold("h-1", admin: admin, participants: new List<User> { admin, participant });
        admin.HouseholdId = household.Id;
        participant.HouseholdId = household.Id;

        _context.Users.AddRange(admin, participant);
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(u => u.FindByIdAsync(admin.Id)).ReturnsAsync(admin);
        _userManagerMock.Setup(u => u.FindByIdAsync(participant.Id)).ReturnsAsync(participant);
        _userManagerMock.Setup(u => u.GetRolesAsync(admin))
            .ReturnsAsync(new List<string> { "HouseholdAdmin" });
        _userManagerMock.Setup(u => u.GetRolesAsync(participant))
            .ReturnsAsync(new List<string> { "Participant" });
        _userManagerMock.Setup(u => u.RemoveFromRoleAsync(admin, "HouseholdAdmin"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.AddToRoleAsync(admin, "Participant"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.AddToRoleAsync(participant, "HouseholdAdmin"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.RemoveFromRoleAsync(participant, "Participant"))
            .ReturnsAsync(IdentityResult.Success);
        _signInManagerMock.Setup(s => s.RefreshSignInAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var result = await _service.SetHouseholdAdminAsync(admin.Id, participant.Id);

        Assert.True(result.IsSuccessful);

        var updated = await _context.Households.FindAsync(household.Id);
        Assert.Equal(participant.Id, updated.AdminId);
    }

    #endregion

    #region ChangeHouseholdCurrencyAsync

    [Fact]
    public async Task ChangeHouseholdCurrencyAsync_UserNotAdmin_ReturnsForbidden()
    {
        var user = TestData.CreateUser();
        var request = TestData.ChangeCurrencyRequest();

        _userManagerMock.Setup(u => u.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Participant" });

        var result = await _service.ChangeHouseholdCurrencyAsync(request, user.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task ChangeHouseholdCurrencyAsync_InvalidCurrencyCode_ReturnsBadRequest()
    {
        var admin = TestData.CreateAdmin();
        var request = TestData.ChangeCurrencyRequest("AB");

        _userManagerMock.Setup(u => u.FindByIdAsync(admin.Id)).ReturnsAsync(admin);
        _userManagerMock.Setup(u => u.GetRolesAsync(admin))
            .ReturnsAsync(new List<string> { "HouseholdAdmin" });

        var result = await _service.ChangeHouseholdCurrencyAsync(request, admin.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Invalid currency code", result.Message);
    }

    [Fact]
    public async Task ChangeHouseholdCurrencyAsync_ValidRequest_ChangesCurrency()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin, currencyCode: "USD");
        admin.HouseholdId = household.Id;

        _context.Users.Add(admin);
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var request = TestData.ChangeCurrencyRequest("EUR");

        _userManagerMock.Setup(u => u.FindByIdAsync(admin.Id)).ReturnsAsync(admin);
        _userManagerMock.Setup(u => u.GetRolesAsync(admin))
            .ReturnsAsync(new List<string> { "HouseholdAdmin" });

        var result = await _service.ChangeHouseholdCurrencyAsync(request, admin.Id);

        Assert.True(result.IsSuccessful);

        var updated = await _context.Households.FindAsync(household.Id);
        Assert.Equal("EUR", updated.CurrencyCode);
    }

    #endregion
}
