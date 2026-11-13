using System.Net;
using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Notifications;
using ChorukoAPI.Contracts.Responses.Invitation;
using ChorukoAPI.Data;
using ChorukoAPI.Mappers;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Service = ChorukoAPI.Services.Implementation.InvitationService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using InvitationService.Test.Helpers;
using ChorukoAPI.Extensions;
using Org.BouncyCastle.Ocsp;

namespace InvitationService.Test;

public class InvitationServiceTests
{
    private readonly DataContext _context;
    private readonly Mock<INotifier> _notifierMock;
    private readonly Mock<IMapper<Invitation, InvitationResponse>> _mapperMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly IInvitationService _service;

    public InvitationServiceTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);
        _notifierMock = new Mock<INotifier>();
        _mapperMock = new Mock<IMapper<Invitation, InvitationResponse>>();
        _userManagerMock = MockUserManager();

        _service = new Service(_context, _notifierMock.Object, _mapperMock.Object, _userManagerMock.Object);
    }

    private Mock<UserManager<User>> MockUserManager()
    {
        var store = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
    }

    #region CreateHouseholdInvitation

    [Fact]
    public async Task CreateHouseholdInvitation_UserNotFound_ReturnsNotFound()
    {
        var result = await _service.CreateHouseholdInvitation("sender-1", "household-1", "none@test.com");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal("No user was found with this email", result.Message);
    }

    [Fact]
    public async Task CreateHouseholdInvitation_UserAlreadyInHousehold_ReturnsBadRequest()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var user = TestData.CreateUser(email: "test@test.com", householdId: household.Id);

        _context.Users.AddRange(admin, user);
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var result = await _service.CreateHouseholdInvitation("sender-1", "any-id", user.Email);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task CreateHouseholdInvitation_HouseholdNotFound_ReturnsNotFound()
    {
        var user = TestData.CreateUser(email: "test@test.com");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _service.CreateHouseholdInvitation("sender-1", "nonexistent", user.Email);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task CreateHouseholdInvitation_ValidRequest_ReturnsSuccess()
    {
        var admin = TestData.CreateAdmin("sender-123");
        var household = TestData.CreateHousehold("household-456", admin: admin);
        var user = TestData.CreateUser(email: "test@test.com");

        _context.Users.AddRange(admin, user);
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        _notifierMock.Setup(p => p.SendAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<AppNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateHouseholdInvitation(admin.Id, household.Id, user.Email);

        Assert.True(result.IsSuccessful);
        Assert.Single(_context.Invitations);
    }

    #endregion

    #region GetAllInvitations

    [Fact]
    public async Task GetAllInvitations_ReturnsInvitationsList()
    {
        var receiver = TestData.CreateUser("user-123");
        var sender = TestData.CreateUser("sender-123", firstName: "Jane", lastName: "Smith");
        var invitation = TestData.CreateInvitation(receiver: receiver, sender: sender);

        _context.Users.AddRange(receiver, sender, invitation.Household.Admin);
        _context.Households.Add(invitation.Household);
        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map(It.IsAny<Invitation>()))
            .ReturnsAsync(new InvitationResponse
            {
                Id = invitation.Id,
                ReceiverId = receiver.Id,
                HouseholdName = invitation.Household.Name,
                HouseholdImagePath = "/images/default.png",
                InviterFirstName = sender.FirstName,
                InviterLastName = sender.LastName,
                Status = invitation.Status
            });

        var result = await _service.GetAllInvitations(receiver.Id);

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Result);
    }

    #endregion

    #region GetInvitation

    [Fact]
    public async Task GetInvitation_InvitationNotFound_ReturnsNotFound()
    {
        var result = await _service.GetInvitation("nonexistent");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task GetInvitation_ValidInvitation_ReturnsSuccess()
    {
        var receiver = TestData.CreateUser("user-123", firstName: "John", lastName: "Doe");
        var sender = TestData.CreateUser("sender-123", firstName: "Jane", lastName: "Smith");
        var invitation = TestData.CreateInvitation("inv-123", receiver: receiver, sender: sender);

        _context.Users.AddRange(receiver, sender, invitation.Household.Admin);
        _context.Households.Add(invitation.Household);
        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map(It.IsAny<Invitation>()))
            .ReturnsAsync(new InvitationResponse
            {
                Id = invitation.Id,
                ReceiverId = receiver.Id,
                HouseholdName = invitation.Household.Name,
                HouseholdImagePath = "/images/default.png",
                InviterFirstName = sender.FirstName,
                InviterLastName = sender.LastName,
                Status = invitation.Status,
            });

        var result = await _service.GetInvitation(invitation.Id);

        Assert.True(result.IsSuccessful);
        Assert.Equal(invitation.Id, result.Result.Id);
    }

    #endregion

    #region AcceptInvitation

    [Fact]
    public async Task AcceptInvitation_InvitationNotFound_ReturnsNotFound()
    {
        var result = await _service.AcceptInvitation("nonexistent");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task AcceptInvitation_UserAlreadyInHousehold_ReturnsConflict()
    {
        var receiver = TestData.CreateUser(householdId: "existing-household");
        var invitation = TestData.CreateInvitation(receiver: receiver);

        _context.Users.AddRange(receiver, invitation.Sender, invitation.Household.Admin);
        _context.Households.Add(invitation.Household);
        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();

        var result = await _service.AcceptInvitation(invitation.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
    }

    [Fact]
    public async Task AcceptInvitation_UserNotInNoneRole_ReturnsForbidden()
    {
        var invitation = TestData.CreateInvitation();

        _context.Users.AddRange(invitation.Receiver, invitation.Sender, invitation.Household.Admin);
        _context.Households.Add(invitation.Household);
        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { "Admin" });

        var result = await _service.AcceptInvitation(invitation.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task AcceptInvitation_ValidRequest_ReturnsSuccess()
    {
        var invitation = TestData.CreateInvitation();

        _context.Users.AddRange(invitation.Receiver, invitation.Sender, invitation.Household.Admin);
        _context.Households.Add(invitation.Household);
        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { "None" });
        _userManagerMock.Setup(u => u.RemoveFromRoleAsync(It.IsAny<User>(), "None"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<User>(), "Participant"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _service.AcceptInvitation(invitation.Id);

        Assert.True(result.IsSuccessful);
        var updated = await _context.Invitations.FindAsync(invitation.Id);
        Assert.Equal(InvitationStatus.Accepted, updated.Status);
    }

    #endregion

    #region DeclineInvitation

    [Fact]
    public async Task DeclineInvitation_InvitationNotFound_ReturnsNotFound()
    {
        var result = await _service.DeclineInvitation("nonexistent");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task DeclineInvitation_AlreadyAccepted_ReturnsBadRequest()
    {
        var invitation = TestData.CreateInvitation(status: InvitationStatus.Accepted);

        _context.Users.AddRange(invitation.Receiver, invitation.Sender, invitation.Household.Admin);
        _context.Households.Add(invitation.Household);
        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();

        var result = await _service.DeclineInvitation(invitation.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task DeclineInvitation_ValidRequest_ReturnsSuccess()
    {
        var invitation = TestData.CreateInvitation();

        _context.Users.AddRange(invitation.Receiver, invitation.Sender, invitation.Household.Admin);
        _context.Households.Add(invitation.Household);
        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();

        var result = await _service.DeclineInvitation(invitation.Id);

        Assert.True(result.IsSuccessful);
        var updated = await _context.Invitations.FindAsync(invitation.Id);
        Assert.Equal(InvitationStatus.Declined, updated.Status);
    }

    #endregion
}
