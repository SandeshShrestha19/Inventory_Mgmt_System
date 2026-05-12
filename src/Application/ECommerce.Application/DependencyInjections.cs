using ECommerce.Application.UseCases;
using ECommerce.Domain.Ports;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    services.AddScoped<ILoginUseCase, LoginUseCase>();
    services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
    services.AddScoped<IAddProductUseCase, AddProductUseCase>();
    services.AddScoped<IUpdateProductUseCase, UpdateProductUseCase>();
    services.AddScoped<IPlaceOrderUseCase, PlaceOrderUseCase>();
    services.AddScoped<IUpdateOrderUseCase, UpdateOrderUseCase>();
    services.AddScoped<IUpdateUserUseCase, UpdateUserUseCase>();
    services.AddScoped<ILogoutUseCase, LogoutUseCase>();
    return services;
  }
}