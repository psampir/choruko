using ChorukoAPI.Models;

namespace InvitationService.Test.Helpers
{
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
                HouseholdId = householdId
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
            string? currencyCode = null)
        {
            var adminUser = admin ?? CreateAdmin();
            return new Household
            {
                Id = id ?? Guid.NewGuid().ToString(),
                Name = name ?? "Test Household",
                AdminId = adminUser.Id,
                Admin = adminUser,
                CurrencyCode = currencyCode ?? "USD",
                Participants = new List<User>()
            };
        }

        // Invitations
        public static Invitation CreateInvitation(
            string? id = null,
            User? receiver = null,
            User? sender = null,
            Household? household = null,
            InvitationStatus? status = null)
        {
            var receiverUser = receiver ?? CreateUser("receiver-1", "receiver@test.com");
            var senderUser = sender ?? CreateUser("sender-1", "sender@test.com");
            var householdData = household ?? CreateHousehold();

            return new Invitation
            {
                Id = id ?? Guid.NewGuid().ToString(),
                ReceiverId = receiverUser.Id,
                Receiver = receiverUser,
                SenderId = senderUser.Id,
                Sender = senderUser,
                HouseholdId = householdData.Id,
                Household = householdData,
                Status = status ?? InvitationStatus.Pending
            };
        }
    }
}
