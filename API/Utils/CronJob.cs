using System;
using System.Threading;
using System.Threading.Tasks;
using API.Models;
using API.Models.Context;
using API.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NET_base.Models.Common;

public class CronJob : BackgroundService
{
    private readonly ILogger<CronJob> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    private Timer _timer;
    private readonly int INTERVAL_SECOND = 15;

    public CronJob(ILogger<CronJob> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(INTERVAL_SECOND));
        return Task.CompletedTask;
    }

    private async void DoWork(object state)
    {
        _logger.LogInformation("Running scheduled task at: {time}", DateTimeOffset.Now);

        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<DBContext>();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Job error: {Message}", e.Message);
        }
    }

    public override Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return base.StopAsync(stoppingToken);
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}