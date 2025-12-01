using ChorukoAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ChorukoAPI.Services.Background;

public class PointsResetService : BackgroundService
{
    private readonly ILogger<PointsResetService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _checkInterval = TimeSpan.FromDays(1);
    private DateTime? _lastResetDate = null; 

    public PointsResetService(ILogger<PointsResetService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Points Reset Service started. Next check in 24 hours.");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_checkInterval, stoppingToken);
            
            await DoWork(stoppingToken);
        }
    }
    
    private async Task DoWork(CancellationToken stoppingToken)
    {
        var now = DateTime.UtcNow.Date;
        
        if (now.Day != 1)
        {
            _logger.LogInformation("Skipping point check. Not the first day of the month. ({Date})", now.ToShortDateString());
            _lastResetDate = null; 
            return;
        }
        
        if (_lastResetDate.HasValue && _lastResetDate.Value.Date == now)
        {
            _logger.LogWarning("Points reset already performed today, skipping execution.");
            return;
        }

        _logger.LogInformation("--- Starting monthly/yearly point reset ---");
        
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        
        bool isMonthlyResetNeeded = true;
        bool isYearlyResetNeeded = (now.Month == 1);

        try
        {
            if (isMonthlyResetNeeded)
            {
                var updatedCount = await context.Users
                    .ExecuteUpdateAsync(sets => sets.SetProperty(u => u.PointsCurrentMonth, 0), stoppingToken);

                _logger.LogInformation("Monthly points reset completed. {Count} users updated.", updatedCount);
            }

            if (isYearlyResetNeeded)
            {
                var updatedCount = await context.Users
                    .ExecuteUpdateAsync(sets => sets.SetProperty(u => u.PointsCurrentYear, 0), stoppingToken);
                
                _logger.LogInformation("Yearly points reset completed. {Count} users updated.", updatedCount);
            }
            
            _lastResetDate = now;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during the Points Reset operation.");
        }
        
        _logger.LogInformation("--- Point reset service finished execution ---");
    }
}
