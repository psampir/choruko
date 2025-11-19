using ChorukoAPI.Data;
using ChorukoAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ChorukoAPI.Services.Background;

public class InvitationsDeleteService(
    ILogger<InvitationsDeleteService> logger,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    private static readonly TimeSpan ExpiryPeriod = TimeSpan.FromDays(7);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            if (now > nextRun)
                nextRun = nextRun.AddDays(1);

            var delay = nextRun - now;
            logger.LogInformation("Next invitations check scheduled in {Delay}", delay);

            await Task.Delay(delay, stoppingToken);

            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DataContext>();

                var declinedInvitations = await db.Invitations
                    .Where(i => i.Status == InvitationStatus.Declined)
                    .ToListAsync(stoppingToken);

                var expiredInvitations = await db.Invitations
                    .Where(i => i.Status == InvitationStatus.Pending && now - i.SentAt > ExpiryPeriod)
                    .ToListAsync(stoppingToken);

                var toRemove = declinedInvitations.Concat(expiredInvitations).ToList();

                if (toRemove.Count > 0)
                    db.Invitations.RemoveRange(toRemove);

                await db.SaveChangesAsync(stoppingToken);
                logger.LogInformation("Removed {Count} invitations", toRemove.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while checking invitations");
            }
        }
    }
}

