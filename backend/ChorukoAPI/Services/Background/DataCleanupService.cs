using ChorukoAPI.Data;
using ChorukoAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ChorukoAPI.Services.Background;

public class DataCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24);

    public DataCleanupService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOldDataAsync(stoppingToken);
            }
            catch
            {
                
            }
            
            if (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }
    }

    private async Task CleanupOldDataAsync(CancellationToken stoppingToken)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();
            
            var cutOffDate = DateTime.UtcNow.AddMonths(-3);
            
            var oldTasks = context.HouseholdTasks
                .Where(t => t.Status == Models.TaskStatus.Completed && t.DeadLine < cutOffDate);

            if (await oldTasks.AnyAsync(stoppingToken))
            {
                context.HouseholdTasks.RemoveRange(oldTasks);
            }
            
            var oldSettlements = context.Settlements
                .Where(s => s.Status == SettlementStatus.Completed && s.DateOfCreation < cutOffDate);

            if (await oldSettlements.AnyAsync(stoppingToken))
            {
                context.Settlements.RemoveRange(oldSettlements);
            }
            
            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync(stoppingToken);
            }
        }
    }
}