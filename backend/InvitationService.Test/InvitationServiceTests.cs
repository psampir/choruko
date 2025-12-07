using System.Net;
using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Responses.Invitation;
using ChorukoAPI.Data;
using ChorukoAPI.Mappers;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Service = ChorukoAPI.Services.Implementation.InvitationService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using ChorukoAPI.Extensions;

namespace InvitationService.Test;

public class InvitationServiceTests
{
    private readonly DataContext _context;
    private readonly Mock<IPushNotificationService> _pushNotificationServiceMock;
    private readonly Mock<IMapper<Invitation, InvitationResponse>> _mapperMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly IInvitationService _service;

    public InvitationServiceTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var configurationMock = new Mock<IConfiguration>().Object;

        _context = new DataContext(options, configurationMock);

        _pushNotificationServiceMock = new Mock<IPushNotificationService>();
        _mapperMock = new Mock<IMapper<Invitation, InvitationResponse>>();
        _userManagerMock = GetMockUserManager();

        _service = new Service(
            _context,
            _pushNotificationServiceMock.Object,
            _mapperMock.Object,
            _userManagerMock.Object
        );
    }

    private Mock<UserManager<User>> GetMockUserManager()
    {
        var store = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
    }

    #region CreateHouseholdInvitation Tests

    [Fact]
    public async Task CreateHouseholdInvitation_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = "sender-123";
        var householdId = "household-456";
        var userEmail = "nonexistent@example.com";

        // Act
        var result = await _service.CreateHouseholdInvitation(userId, householdId, userEmail);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal("No users were found", result.Message);
    }

    [Fact]
    public async Task CreateHouseholdInvitation_UserAlreadyInHousehold_ReturnsBadRequest()
    {
        // Arrange
        var userId = "sender-123";
        var householdId = "household-456";
        var userEmail = "test@example.com";

        var admin = new User
        {
            Id = "admin-1",
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User"
        };

        var household = new Household
        {
            Id = "existing-household",
            Name = "Existing",
            AdminId = admin.Id,
            Admin = admin
        };

        var existingUser = new User
        {
            Id = "user-789",
            Email = userEmail,
            HouseholdId = household.Id,
            FirstName = "John",
            LastName = "Doe"
        };

        _context.Users.Add(admin);
        _context.Households.Add(household);
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CreateHouseholdInvitation(userId, householdId, userEmail);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("This user already belongs to a household", result.Message);
    }

    [Fact]
    public async Task CreateHouseholdInvitation_HouseholdNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = "sender-123";
        var householdId = "nonexistent-household";
        var userEmail = "test@example.com";

        var user = new User
        {
            Id = "user-789",
            Email = userEmail,
            HouseholdId = null,
            FirstName = "John",
            LastName = "Doe"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CreateHouseholdInvitation(userId, householdId, userEmail);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal("No households were found", result.Message);
    }

    [Fact]
    public async Task CreateHouseholdInvitation_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var userId = "sender-123";
        var householdId = "household-456";
        var userEmail = "test@example.com";

        var admin = new User
        {
            Id = userId,
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User"
        };

        var userToInvite = new User
        {
            Id = "user-789",
            Email = userEmail,
            HouseholdId = null,
            FirstName = "John",
            LastName = "Doe"
        };

        var household = new Household
        {
            Id = householdId,
            Name = "Test Household",
            AdminId = admin.Id,
            Admin = admin,
            Participants = new List<User>()
        };

        _context.Users.Add(admin);
        _context.Users.Add(userToInvite);
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        _pushNotificationServiceMock
            .Setup(p => p.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ServiceResponseFactory.Success(
                new OperationResultDto { Operation = "SendNotification" },
                "Notification sent"
            ));

        // Act
        var result = await _service.CreateHouseholdInvitation(userId, householdId, userEmail);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Result);
        Assert.Equal("CreateHouseholdInvitation", result.Result.Operation);
        Assert.Single(_context.Invitations);
    }

    #endregion

    #region GetAllInvitations Tests

    [Fact]
    public async Task GetAllInvitations_ReturnsInvitationsList()
    {
        // Arrange
        var userId = "user-123";

        var admin = new User
        {
            Id = "admin-1",
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User"
        };

        var receiver = new User
        {
            Id = userId,
            Email = "user@test.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var sender = new User
        {
            Id = "sender-1",
            Email = "sender@test.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        var household = new Household
        {
            Id = "h-1",
            Name = "Test Household",
            AdminId = admin.Id,
            Admin = admin
        };

        var invitation = new Invitation
        {
            Id = "inv-1",
            ReceiverId = userId,
            Status = InvitationStatus.Pending,
            Receiver = receiver,
            SenderId = sender.Id,
            Sender = sender,
            HouseholdId = household.Id,
            Household = household
        };

        _context.Users.Add(admin);
        _context.Users.Add(receiver);
        _context.Users.Add(sender);
        _context.Households.Add(household);
        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map(It.IsAny<Invitation>()))
            .ReturnsAsync(new InvitationResponse
            {
                Id = "inv-1",
                ReceiverId = userId,
                HouseholdName = "Test Household"
            });

        // Act
        var result = await _service.GetAllInvitations(userId);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Result);
        Assert.Single(result.Result);
    }

    #endregion

    #region GetInvitation Tests

    [Fact]
    public async Task GetInvitation_InvitationNotFound_ReturnsNotFound()
    {
        // Arrange
        var invitationId = "nonexistent-inv";

        // Act
        var result = await _service.GetInvitation(invitationId);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal("No invitation were found", result.Message);
    }

    [Fact]
    public async Task GetInvitation_ValidInvitation_ReturnsSuccess()
    {
        // Arrange
        var invitationId = "inv-123";

        var admin = new User
        {
            Id = "admin-1",
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User"
        };

        var receiver = new User
        {
            Id = "user-123",
            Email = "user@test.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var sender = new User
        {
            Id = "sender-1",
            Email = "sender@test.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        var household = new Household
        {
            Id = "h-1",
            Name = "Test Household",
            AdminId = admin.Id,
            Admin = admin
        };

        var invitation = new Invitation
        {
            Id = invitationId,
            ReceiverId = receiver.Id,
            Status = InvitationStatus.Pending,
            Receiver = receiver,
            SenderId = sender.Id,
            Sender = sender,
            HouseholdId = household.Id,
            Household = household
        };

        _context.Users.Add(admin);
        _context.Users.Add(receiver);
        _context.Users.Add(sender);
        _context.Households.Add(household);
        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map(It.IsAny<Invitation>()))
            .ReturnsAsync(new InvitationResponse
            {
                Id = invitationId,
                ReceiverId = "user-123",
                HouseholdName = "Test Household"
            });

        // Act
        var result = await _service.GetInvitation(invitationId);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Result);
        Assert.Equal(invitationId, result.Result.Id);
    }

    #endregion

    #region AcceptInvitation Tests

    [Fact]
    public async Task AcceptInvitation_InvitationNotFound_ReturnsNotFound()
    {
        // Arrange
        var invitationId = "nonexistent-inv";

        // Act
        var result = await _service.AcceptInvitation(invitationId);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal("No invitation were found", result.Message);
    }

    [Fact]
    public async Task AcceptInvitation_UserAlreadyInHousehold_ReturnsConflict()
    {
        // Arrange
        var invitationId = "inv-123";
        var receiverId = "user-123";

        var admin = new User
        {
            Id = "admin-1",
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User"
        };

        var household = new Household
        {
            Id = "h-1",
            Name = "Test Household",
            AdminId = admin.Id,
            Admin = admin,
            Participants = new List<User>()
        };

        var receiver = new User
        {
            Id = receiverId,
            Email = "user@test.com",
            HouseholdId = "existing-household",
            FirstName = "John",
            LastName = "Doe"
        };

        var invitation = new Invitation
        {
            Id = invitationId,
            ReceiverId = receiverId,
            HouseholdId = household.Id,
            Status = InvitationStatus.Pending,
            Receiver = receiver,
            Sender = admin,
            SenderId = admin.Id,
            Household = household
        };

        _context.Users.Add(admin);
        _context.Users.Add(receiver);
        _context.Households.Add(household);
        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AcceptInvitation(invitationId);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        Assert.Equal("You already belong to a household.", result.Message);
    }

    [Fact]
    public async Task AcceptInvitation_UserNotInNoneRole_ReturnsForbidden()
    {
        // Arrange
        var invitationId = "inv-123";
        var receiverId = "user-123";

        var admin = new User
        {
            Id = "admin-1",
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User"
        };

        var household = new Household
        {
            Id = "h-1",
            Name = "Test Household",
            AdminId = admin.Id,
            Admin = admin,
            Participants = new List<User>()
        };

        var receiver = new User
        {
            Id = receiverId,
            Email = "user@test.com",
            HouseholdId = null,
            FirstName = "John",
            LastName = "Doe"
        };

        var invitation = new Invitation
        {
            Id = invitationId,
            ReceiverId = receiverId,
            HouseholdId = household.Id,
            Status = InvitationStatus.Pending,
            Receiver = receiver,
            SenderId = admin.Id,
            Sender = admin,
            Household = household
        };

        _context.Users.Add(admin);
        _context.Users.Add(receiver);
        _context.Households.Add(household);
        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { "Admin" });

        // Act
        var result = await _service.AcceptInvitation(invitationId);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        Assert.Equal("Only users with the 'None' role can be added to a household.", result.Message);
    }

    [Fact]
    public async Task AcceptInvitation_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var invitationId = "inv-123";
        var receiverId = "user-123";

        var admin = new User
        {
            Id = "admin-1",
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User"
        };

        var household = new Household
        {
            Id = "h-1",
            Name = "Test Household",
            AdminId = admin.Id,
            Admin = admin,
            Participants = new List<User>()
        };

        var receiver = new User
        {
            Id = receiverId,
            Email = "user@test.com",
            HouseholdId = null,
            FirstName = "John",
            LastName = "Doe"
        };

        var invitation = new Invitation
        {
            Id = invitationId,
            ReceiverId = receiverId,
            HouseholdId = household.Id,
            Status = InvitationStatus.Pending,
            Receiver = receiver,
            SenderId = admin.Id,
            Sender = admin,
            Household = household
        };

        _context.Users.Add(admin);
        _context.Users.Add(receiver);
        _context.Households.Add(household);
        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { "None" });

        _userManagerMock.Setup(u => u.RemoveFromRoleAsync(It.IsAny<User>(), "None"))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<User>(), "Participant"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _service.AcceptInvitation(invitationId);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Result);
        Assert.Equal("AcceptInvitation", result.Result.Operation);
        Assert.Equal(InvitationStatus.Accepted, invitation.Status);
        Assert.Equal(household.Id, receiver.HouseholdId);
    }


    #endregion

    #region DeclineInvitation Tests

    [Fact]
    public async Task DeclineInvitation_InvitationNotFound_ReturnsNotFound()
    {
        // Arrange
        var invitationId = "nonexistent-inv";

        // Act
        var result = await _service.DeclineInvitation(invitationId);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task DeclineInvitation_AlreadyAccepted_ReturnsBadRequest()
    {
        // Arrange
        var invitationId = "inv-123";

        var admin = new User
        {
            Id = "admin-1",
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User"
        };

        var receiver = new User
        {
            Id = "user-123",
            Email = "user@test.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var sender = new User
        {
            Id = "sender-1",
            Email = "sender@test.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        var household = new Household
        {
            Id = "h-1",
            Name = "Test Household",
            AdminId = admin.Id,
            Admin = admin
        };

        var invitation = new Invitation
        {
            Id = invitationId,
            Status = InvitationStatus.Accepted,
            ReceiverId = receiver.Id,
            Receiver = receiver,
            SenderId = sender.Id,
            Sender = sender,
            HouseholdId = household.Id,
            Household = household
        };

        _context.Users.Add(admin);
        _context.Users.Add(receiver);
        _context.Users.Add(sender);
        _context.Households.Add(household);
        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeclineInvitation(invitationId);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("You accepted this invitation", result.Message);
    }

    [Fact]
    public async Task DeclineInvitation_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var invitationId = "inv-123";

        var admin = new User
        {
            Id = "admin-1",
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User"
        };

        var receiver = new User
        {
            Id = "user-123",
            Email = "user@test.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var sender = new User
        {
            Id = "sender-1",
            Email = "sender@test.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        var household = new Household
        {
            Id = "h-1",
            Name = "Test Household",
            AdminId = admin.Id,
            Admin = admin
        };

        var invitation = new Invitation
        {
            Id = invitationId,
            ReceiverId = receiver.Id,
            Status = InvitationStatus.Pending,
            Receiver = receiver,
            SenderId = sender.Id,
            Sender = sender,
            HouseholdId = household.Id,
            Household = household
        };

        _context.Users.Add(admin);
        _context.Users.Add(receiver);
        _context.Users.Add(sender);
        _context.Households.Add(household);
        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeclineInvitation(invitationId);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Result);
        Assert.Equal("DeclineInvitation", result.Result.Operation);
        Assert.Equal(InvitationStatus.Declined, invitation.Status);
    }

    #endregion
}
