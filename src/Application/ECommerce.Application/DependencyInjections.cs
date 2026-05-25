using ECommerce.Applcation.Helpers;
using ECommerce.Application.UseCase;
using ECommerce.Application.UseCases;
using ECommerce.Domain.Ports;
using ECommerce.Domain.UseCase;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
  public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
  {
    services.AddScoped<ILoginUseCase, LoginUseCase>();
    services.AddScoped<ILoginWith2FAUseCase, LoginWith2FAUseCase>();
    services.AddScoped<ILogoutUseCase, LogoutUseCase>();
    services.AddScoped<IEnable2FAUseCase, Enable2FAUseCase>();
    services.AddScoped<IVerify2FAUseCase, Verify2FAUseCase>();

    services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
    services.AddScoped<IAddProductUseCase, AddProductUseCase>();
    services.AddScoped<IPlaceOrderUseCase, PlaceOrderUseCase>();

    services.AddScoped<IUpdateProductUseCase, UpdateProductUseCase>();
    services.AddScoped<IUpdateOrderUseCase, UpdateOrderUseCase>();
    services.AddScoped<IUpdateUserUseCase, UpdateUserUseCase>();
    
    services.AddScoped<IRefreshTokenUseCase, RefreshTokenUseCase>();
    services.AddScoped<ISetUserActiveStatusUseCase, SetUserActiveStatusUseCase>();
    services.AddScoped<JwtTokenGenerator>();
    services.AddScoped<TwoFactorService>();
    services.AddScoped<IBlacklistedTokenUseCase, BlacklistedTokenUseCase>();

    services.AddScoped<IGetProductQueryUseCase, GetProductQueryUseCase>();
    services.AddScoped<IGetUserQueryUseCase, GetUserQueryUseCase>();
    services.AddScoped<IGetOrderQueryUseCase, GetOrderQueryUseCase>();

    services.AddScoped<IOrderResponseUseCase, OrderResponseUseCase>();
    services.AddScoped<IProductResponseUseCase, ProductResponseUseCase>();
    services.AddScoped<IUserResponseUseCase, UserResponseUseCase>();

    services.AddScoped<IUserFacade, UserFacade>();
    services.AddScoped<IProductFacade, ProductFacade>();
    services.AddScoped<IOrderFacade, OrderFacade>();

    services.AddScoped<IDeleteUserUseCase, DeleteUserUseCase>();
    services.AddScoped<IDeleteProductUseCase, DeleteProductUseCase>();
    services.AddScoped<IDeleteOrderUseCase, DeleteOrderUseCase>();

    services.AddScoped<IIncreaseProductStockUseCase, IncreaseProductStockUseCase>();
    services.AddScoped<IDecreaseProductStockUseCase, DecreaseProductStockUseCase>();

    return services;
  }
}