using System.Net;
using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Settlement;
using ChorukoAPI.Contracts.Responses.Settlement;
using ChorukoAPI.Data;
using ChorukoAPI.Extensions;
using ChorukoAPI.Mappers;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Service = ChorukoAPI.Services.Implementation.SettlementService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using SettlementService.Test.Helpers;

namespace SettlementService.Test;

public class SettlementServiceTests : IDisposable
{
    private readonly DataContext _context;
    private readonly Mock<IMapper<Settlement, SettlementResponse>> _mapperMock;
    private readonly Mock<IMapper<Settlement, SettlementWithoutUsersResponse>> _withoutUsersMapperMock;
    private readonly Mock<IPushNotificationService> _pushMock;
    private readonly ISettlementService _service;

    public SettlementServiceTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);
        _mapperMock = new Mock<IMapper<Settlement, SettlementResponse>>();
        _withoutUsersMapperMock = new Mock<IMapper<Settlement, SettlementWithoutUsersResponse>>();
        _pushMock = new Mock<IPushNotificationService>();

        InitializeNotificationHandler();

        _service = new Service(_context, _mapperMock.Object, _withoutUsersMapperMock.Object, _pushMock.Object);
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

    public void Dispose()
    {
        _context?.Dispose();
    }

    #region CreateSettlement

    [Fact]
    public async Task CreateSettlement_HouseholdNotFound_ReturnsNotFound()
    {
        var request = TestData.CreateSettlementRequest();

        var result = await _service.CreateSettlement("nonexistent-household", request, "user-1");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Contains("Household not found", result.Message);
    }

    [Fact]
    public async Task CreateSettlement_AmountZeroOrNegative_ReturnsBadRequest()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var request = TestData.CreateSettlementRequest(amount: 0);

        _context.Users.Add(admin);
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var result = await _service.CreateSettlement(household.Id, request, admin.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("greater than 0", result.Message);
    }

    [Fact]
    public async Task CreateSettlement_CreatorInUnsettledList_ReturnsBadRequest()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var request = TestData.CreateSettlementRequest(unsettledUsers: new List<string> { admin.Id });

        _context.Users.Add(admin);
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var result = await _service.CreateSettlement(household.Id, request, admin.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("cannot assign himself", result.Message);
    }

    [Fact]
    public async Task CreateSettlement_InvalidParticipantIds_ReturnsBadRequest()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var request = TestData.CreateSettlementRequest(
            unsettledUsers: new List<string> { "invalid-user-1", "invalid-user-2" }
        );

        _context.Users.Add(admin);
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var result = await _service.CreateSettlement(household.Id, request, admin.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("not participants of the household", result.Message);
    }

    [Fact]
    public async Task CreateSettlement_ValidRequest_CreatesSettlement()
    {
        var admin = TestData.CreateAdmin();
        var user1 = TestData.CreateUser("user-1", "user1@test.com");
        var user2 = TestData.CreateUser("user-2", "user2@test.com");
        var household = TestData.CreateHousehold(
            admin: admin,
            participants: new List<User> { admin, user1, user2 }
        );
        var request = TestData.CreateSettlementRequest(
            description: "Dinner bill",
            amount: 150.50m,
            unsettledUsers: new List<string> { user1.Id, user2.Id }
        );

        _context.Users.AddRange(admin, user1, user2);
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var result = await _service.CreateSettlement(household.Id, request, admin.Id);

        Assert.True(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        Assert.Equal("CreateSettlement", result.Result.Operation);

        var settlement = await _context.Settlements
            .Include(s => s.UnsettledUsers)
            .FirstOrDefaultAsync();
        Assert.NotNull(settlement);
        Assert.Equal("Dinner bill", settlement.Description);
        Assert.Equal(150.50m, settlement.Amount);
        Assert.Equal(2, settlement.UnsettledUsers.Count);
        Assert.Equal(SettlementStatus.Pending, settlement.Status);
    }

    #endregion

    #region EditSettlement

    [Fact]
    public async Task EditSettlement_SettlementNotFound_ReturnsNotFound()
    {
        var request = TestData.EditSettlementRequest();

        var result = await _service.EditSettlement("nonexistent-settlement", request, "user-1");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task EditSettlement_UserNotCreator_ReturnsNotFound()
    {
        var admin = TestData.CreateAdmin();
        var otherUser = TestData.CreateUser("other-user");
        var household = TestData.CreateHousehold(admin: admin);
        var settlement = TestData.CreateSettlement(household: household, creator: admin);

        _context.Users.AddRange(admin, otherUser);
        _context.Households.Add(household);
        _context.Settlements.Add(settlement);
        await _context.SaveChangesAsync();

        var request = TestData.EditSettlementRequest();

        var result = await _service.EditSettlement(settlement.Id, request, otherUser.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Contains("not a creator", result.Message);
    }

    [Fact]
    public async Task EditSettlement_CreatorInUnsettledList_ReturnsBadRequest()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var settlement = TestData.CreateSettlement(household: household, creator: admin);

        _context.Users.Add(admin);
        _context.Households.Add(household);
        _context.Settlements.Add(settlement);
        await _context.SaveChangesAsync();

        var request = TestData.EditSettlementRequest(
            unsettledUsers: new List<string> { admin.Id }
        );

        var result = await _service.EditSettlement(settlement.Id, request, admin.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task EditSettlement_ValidRequest_UpdatesSettlement()
    {
        var admin = TestData.CreateAdmin();
        var user1 = TestData.CreateUser("user-1");
        var user2 = TestData.CreateUser("user-2");
        var household = TestData.CreateHousehold(
            admin: admin,
            participants: new List<User> { admin, user1, user2 }
        );
        var settlement = TestData.CreateSettlement(
            description: "Original",
            amount: 100m,
            household: household,
            creator: admin,
            unsettledUsers: new List<User> { user1 }
        );

        _context.Users.AddRange(admin, user1, user2);
        _context.Households.Add(household);
        _context.Settlements.Add(settlement);
        await _context.SaveChangesAsync();

        var request = TestData.EditSettlementRequest(
            description: "Updated",
            amount: 200m,
            unsettledUsers: new List<string> { user1.Id, user2.Id }
        );

        var result = await _service.EditSettlement(settlement.Id, request, admin.Id);

        Assert.True(result.IsSuccessful);

        var updated = await _context.Settlements
            .Include(s => s.UnsettledUsers)
            .Include(s => s.SettledUsers)
            .FirstOrDefaultAsync(s => s.Id == settlement.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated.Description);
        Assert.Equal(200m, updated.Amount);
        Assert.Equal(2, updated.UnsettledUsers.Count);
        Assert.Empty(updated.SettledUsers);
    }

    #endregion

    #region DeleteSettlement

    [Fact]
    public async Task DeleteSettlement_SettlementNotFound_ReturnsNotFound()
    {
        var result = await _service.DeleteSettlement("nonexistent", "user-1");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task DeleteSettlement_UserNotCreator_ReturnsBadRequest()
    {
        var admin = TestData.CreateAdmin();
        var otherUser = TestData.CreateUser("other-user");
        var household = TestData.CreateHousehold(admin: admin);
        var settlement = TestData.CreateSettlement(household: household, creator: admin);

        _context.Users.AddRange(admin, otherUser);
        _context.Households.Add(household);
        _context.Settlements.Add(settlement);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteSettlement(settlement.Id, otherUser.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task DeleteSettlement_ValidRequest_DeletesSettlement()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var settlement = TestData.CreateSettlement(household: household, creator: admin);

        _context.Users.Add(admin);
        _context.Households.Add(household);
        _context.Settlements.Add(settlement);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteSettlement(settlement.Id, admin.Id);

        Assert.True(result.IsSuccessful);

        var deleted = await _context.Settlements.FindAsync(settlement.Id);
        Assert.Null(deleted);
    }

    #endregion


    #region GetAllSettlements

    [Fact]
    public async Task GetAllSettlements_NoSettlements_ReturnsEmptyList()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);

        _context.Users.Add(admin);
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var result = await _service.GetAllSettlements(household.Id);

        Assert.True(result.IsSuccessful);
        Assert.Empty(result.Result);
    }

    [Fact]
    public async Task GetAllSettlements_MultipleSettlements_ReturnsAll()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var settlement1 = TestData.CreateSettlement("s-1", "Settlement 1", household: household, creator: admin);
        var settlement2 = TestData.CreateSettlement("s-2", "Settlement 2", household: household, creator: admin);

        _context.Users.Add(admin);
        _context.Households.Add(household);
        _context.Settlements.AddRange(settlement1, settlement2);
        await _context.SaveChangesAsync();

        _withoutUsersMapperMock.Setup(m => m.Map(It.IsAny<Settlement>()))
            .ReturnsAsync((Settlement s) => TestData.CreateSettlementWithoutUsersResponse(
                id: s.Id,
                householdId: s.HouseholdId,
                description: s.Description,
                amount: s.Amount,
                status: s.Status
            ));

        var result = await _service.GetAllSettlements(household.Id);

        Assert.True(result.IsSuccessful);
        Assert.Equal(2, result.Result.Count);
    }

    #endregion

    #region GetSettlement

    [Fact]
    public async Task GetSettlement_SettlementNotFound_ReturnsNotFound()
    {
        var result = await _service.GetSettlement("nonexistent");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task GetSettlement_ValidSettlement_ReturnsSettlement()
    {
        var admin = TestData.CreateAdmin();
        var household = TestData.CreateHousehold(admin: admin);
        var settlement = TestData.CreateSettlement("s-123", household: household, creator: admin);

        _context.Users.Add(admin);
        _context.Households.Add(household);
        _context.Settlements.Add(settlement);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map(It.IsAny<Settlement>()))
            .ReturnsAsync(TestData.CreateSettlementResponse(id: settlement.Id));

        var result = await _service.GetSettlement(settlement.Id);

        Assert.True(result.IsSuccessful);
        Assert.Equal(settlement.Id, result.Result.Id);
    }

    #endregion

    #region MarkAsDone

    [Fact]
    public async Task MarkAsDone_SettlementNotFound_ReturnsNotFound()
    {
        var result = await _service.MarkAsDone("nonexistent", "user-1");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task MarkAsDone_UserNotCreator_ReturnsNotFound()
    {
        var admin = TestData.CreateAdmin();
        var otherUser = TestData.CreateUser("other-user");
        var household = TestData.CreateHousehold(admin: admin);
        var settlement = TestData.CreateSettlement(household: household, creator: admin);

        _context.Users.AddRange(admin, otherUser);
        _context.Households.Add(household);
        _context.Settlements.Add(settlement);
        await _context.SaveChangesAsync();

        var result = await _service.MarkAsDone(settlement.Id, otherUser.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task MarkAsDone_ValidRequest_MarksAsCompleted()
    {
        var admin = TestData.CreateAdmin();
        var user1 = TestData.CreateUser("user-1");
        var household = TestData.CreateHousehold(admin: admin);
        var settlement = TestData.CreateSettlement(
            household: household,
            creator: admin,
            unsettledUsers: new List<User> { user1 },
            status: SettlementStatus.Pending
        );

        _context.Users.AddRange(admin, user1);
        _context.Households.Add(household);
        _context.Settlements.Add(settlement);
        await _context.SaveChangesAsync();

        _pushMock.Setup(p => p.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ServiceResponseFactory.Success(new OperationResultDto(), "OK"));

        var result = await _service.MarkAsDone(settlement.Id, admin.Id);

        Assert.True(result.IsSuccessful);

        var updated = await _context.Settlements
            .Include(s => s.UnsettledUsers)
            .Include(s => s.SettledUsers)
            .FirstOrDefaultAsync(s => s.Id == settlement.Id);
        Assert.NotNull(updated);
        Assert.Equal(SettlementStatus.Completed, updated.Status);
        Assert.Empty(updated.UnsettledUsers);
        Assert.Single(updated.SettledUsers);
    }

    #endregion

    #region UnMarkAsDone

    [Fact]
    public async Task UnMarkAsDone_ValidRequest_ReopensSettlement()
    {
        var admin = TestData.CreateAdmin();
        var user1 = TestData.CreateUser("user-1");
        var household = TestData.CreateHousehold(admin: admin);
        var settlement = TestData.CreateSettlement(
            household: household,
            creator: admin,
            settledUsers: new List<User> { user1 },
            status: SettlementStatus.Completed
        );

        _context.Users.AddRange(admin, user1);
        _context.Households.Add(household);
        _context.Settlements.Add(settlement);
        await _context.SaveChangesAsync();

        _pushMock.Setup(p => p.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ServiceResponseFactory.Success(new OperationResultDto(), "OK"));

        var result = await _service.UnMarkAsDone(settlement.Id, admin.Id);

        Assert.True(result.IsSuccessful);

        var updated = await _context.Settlements
            .Include(s => s.UnsettledUsers)
            .Include(s => s.SettledUsers)
            .FirstOrDefaultAsync(s => s.Id == settlement.Id);
        Assert.NotNull(updated);
        Assert.Equal(SettlementStatus.Pending, updated.Status);
        Assert.Single(updated.UnsettledUsers);
        Assert.Empty(updated.SettledUsers);
    }

    #endregion

    #region MarkAsPaid

    [Fact]
    public async Task MarkAsPaid_UserNotFound_ReturnsNotFound()
    {
        var result = await _service.MarkAsPaid("settlement-1", "nonexistent-user");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task MarkAsPaid_UserNotInUnsettled_ReturnsNotFound()
    {
        var admin = TestData.CreateAdmin();
        var user1 = TestData.CreateUser("user-1");
        var household = TestData.CreateHousehold(admin: admin);
        var settlement = TestData.CreateSettlement(
            household: household,
            creator: admin,
            unsettledUsers: new List<User>()
        );

        _context.Users.AddRange(admin, user1);
        _context.Households.Add(household);
        _context.Settlements.Add(settlement);
        await _context.SaveChangesAsync();

        var result = await _service.MarkAsPaid(settlement.Id, user1.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task MarkAsPaid_ValidRequest_MovesToSettled()
    {
        var admin = TestData.CreateAdmin();
        var user1 = TestData.CreateUser("user-1");
        var household = TestData.CreateHousehold(admin: admin);
        var settlement = TestData.CreateSettlement(
            household: household,
            creator: admin,
            unsettledUsers: new List<User> { user1 }
        );

        _context.Users.AddRange(admin, user1);
        _context.Households.Add(household);
        _context.Settlements.Add(settlement);
        await _context.SaveChangesAsync();

        _pushMock.Setup(p => p.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ServiceResponseFactory.Success(new OperationResultDto(), "OK"));

        var result = await _service.MarkAsPaid(settlement.Id, user1.Id);

        Assert.True(result.IsSuccessful);

        var updated = await _context.Settlements
            .Include(s => s.UnsettledUsers)
            .Include(s => s.SettledUsers)
            .FirstOrDefaultAsync(s => s.Id == settlement.Id);
        Assert.NotNull(updated);
        Assert.Empty(updated.UnsettledUsers);
        Assert.Single(updated.SettledUsers);
        Assert.Equal(SettlementStatus.Completed, updated.Status);
    }

    #endregion

    #region MarAsUnPaid

    [Fact]
    public async Task MarAsUnPaid_ValidRequest_MovesToUnsettled()
    {
        var admin = TestData.CreateAdmin();
        var user1 = TestData.CreateUser("user-1");
        var household = TestData.CreateHousehold(admin: admin);
        var settlement = TestData.CreateSettlement(
            household: household,
            creator: admin,
            settledUsers: new List<User> { user1 },
            status: SettlementStatus.Completed
        );

        _context.Users.AddRange(admin, user1);
        _context.Households.Add(household);
        _context.Settlements.Add(settlement);
        await _context.SaveChangesAsync();

        _pushMock.Setup(p => p.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ServiceResponseFactory.Success(new OperationResultDto(), "OK"));

        var result = await _service.MarAsUnPaid(settlement.Id, user1.Id);

        Assert.True(result.IsSuccessful);

        var updated = await _context.Settlements
            .Include(s => s.UnsettledUsers)
            .Include(s => s.SettledUsers)
            .FirstOrDefaultAsync(s => s.Id == settlement.Id);
        Assert.NotNull(updated);
        Assert.Single(updated.UnsettledUsers);
        Assert.Empty(updated.SettledUsers);
        Assert.Equal(SettlementStatus.Pending, updated.Status);
    }

    #endregion
}
