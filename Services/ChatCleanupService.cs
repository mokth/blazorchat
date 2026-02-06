using blazorchat.Data;
using blazorchat.Models;
using Microsoft.EntityFrameworkCore;

namespace blazorchat.Services;

public class ChatCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatCleanupService> _logger;

    public ChatCleanupService(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<ChatCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMinutes = _configuration.GetValue<int>("ChatRetention:CleanupIntervalMinutes");
        var delay = TimeSpan.FromMinutes(Math.Max(intervalMinutes, 5));

        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupOldMessagesAsync(stoppingToken);
            await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task CleanupOldMessagesAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ChatDbContext>>();
            var fileUploadService = scope.ServiceProvider.GetRequiredService<FileUploadService>();

            var cleanupAfterDays = _configuration.GetValue<int>("ChatRetention:CleanupAfterDays");
            var cutoff = DateTime.UtcNow.AddDays(-cleanupAfterDays);

            await using var dbContext = await dbContextFactory.CreateDbContextAsync(stoppingToken);
            var oldMessages = await dbContext.ChatMessages
                .Where(message => message.Timestamp < cutoff)
                .ToListAsync(stoppingToken);

            if (oldMessages.Count == 0)
            {
                return;
            }

            foreach (var message in oldMessages)
            {
                if (message.Type != MessageType.Text && !string.IsNullOrWhiteSpace(message.Content))
                {
                    fileUploadService.DeleteFileIfExists(message.Content);
                }
            }

            dbContext.ChatMessages.RemoveRange(oldMessages);
            await dbContext.SaveChangesAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old chat messages.");
        }
    }
}
