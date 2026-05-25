using ECommerce.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

public class UserActivityJob : BackgroundService
{
  private readonly IServiceScopeFactory _serviceScopeFactory; //creates scope to access DB
  private readonly ILogger<UserActivityJob> _logger;
  private readonly TimeSpan _interval = TimeSpan.FromMinutes(7);
  public UserActivityJob(IServiceScopeFactory serviceScopeFactory, ILogger<UserActivityJob> logger)
  {
    _serviceScopeFactory = serviceScopeFactory;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken) //stoppingToken stops job when app shuts down
  {
    _logger.LogInformation("UserActivityJob started!");

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        await DoWorkAsync();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "UserActivityJob failed!");
      }

      await Task.Delay(_interval, stoppingToken); // wait 5 mins then run again
    }
  }

  private async Task DoWorkAsync()
  {
    using var scope = _serviceScopeFactory.CreateScope(); //create scope to db
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>(); //access dbContext

    var inactiveTokens = await context.RefreshTokens.Where(x => x.IsRevoked == true || x.ExpiresIn<DateTime.UtcNow).ToListAsync();
    
    var inactiveUserIds = inactiveTokens.Select(x => x.Id).Distinct().ToList();

    foreach(var userId in inactiveUserIds)
    {
      var hasActiveToken = await context.RefreshTokens.AnyAsync(x => x.UserId == userId && x.IsRevoked == false && x.ExpiresIn > DateTime.UtcNow);

      if (!hasActiveToken)
      {
        var user = await context.Users.FindAsync(userId);
        if(user != null && user.IsLoggedIn)
        {
          user.IsLoggedIn = false;
          _logger.LogInformation($"User {user.Email} set to inactive!");
        }
      }
    }
    var expiredTokens = await context.BlacklistedTokens.Where(x => x.ExpiresAt < DateTime.UtcNow).ToListAsync();

    if (expiredTokens.Any())
    {
      context.BlacklistedTokens.RemoveRange(expiredTokens);
      _logger.LogInformation($"Cleaned up {expiredTokens.Count()} expired blacklisted tokens!");
    }

    await context.SaveChangesAsync();
  }
}