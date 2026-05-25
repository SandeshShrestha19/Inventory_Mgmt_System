using ECommerce.Domain.Ports;
using ECommerce.Infrastructure.Adapters;
using ECommerce.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastrutureDependencies(this IServiceCollection services)
  {
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    services.AddScoped<IProductRepository, ProductAdapter>();
    services.AddScoped<IOrderRepository, OrderAdapter>();
    services.AddScoped<IUserRepository, UserAdapter>();
    services.AddScoped<IRefreshTokenRepository, RefreshTokenAdapter>();
    services.AddScoped<IBlacklistedTokenRepository, BlacklistedTokenAdapter>();
    services.AddScoped<UserActivityJob>();
    return services;
  }
}