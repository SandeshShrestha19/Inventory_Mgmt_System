using ECommerce.Applcation.Helpers;
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
    services.AddScoped<IOrderResponseUseCase, OrderResponseUseCase>();
    services.AddScoped<IProductResponseUseCase, ProductResponseUseCase>();
    services.AddScoped<IUserResponseUseCase, UserResponseUseCase>();
    services.AddScoped<IUpdateUserUseCase, UpdateUserUseCase>();
    services.AddScoped<IGetProductQueryUseCase, GetProductQueryUseCase>();
    services.AddScoped<IGetUserQueryUseCase, GetUserQueryUseCase>();
    services.AddScoped<IGetOrderQueryUseCase, GetOrderQueryUseCase>();
    services.AddScoped<ILogoutUseCase, LogoutUseCase>();
    services.AddScoped<IRefreshTokenUseCase, RefreshTokenUseCase>();
    services.AddScoped<ISetUserActiveStatusUseCase, SetUserActiveStatusUseCase>();
    services.AddScoped<JwtTokenGenerator>();
    return services;
  }
}