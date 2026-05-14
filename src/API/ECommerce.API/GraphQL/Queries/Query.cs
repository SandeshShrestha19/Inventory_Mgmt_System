using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;
using HotChocolate.Authorization;

namespace ECommerce.API.GraphQL.Queries;

public class Query
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Product> GetProducts([Service] IProductRepository repo) =>
        repo.GetAllAsync();

    public async Task<ProductResponseModel?> GetProductById([Service] IProductResponseUseCase productResponseUseCase, Guid id) =>
        await productResponseUseCase.ExecuteAsync(id);  

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize]
    public IQueryable<Order> GetOrders([Service] IOrderRepository repo) =>
        repo.GetAllAsync();

    [Authorize]
    public async Task<OrderResponseModel?> GetOrderById([Service] IOrderResponseUseCase orderResponseUseCase, Guid id) =>
        await orderResponseUseCase.ExecuteAsync(id);

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize(Roles = ["Admin", "Manager"])]
    public IQueryable<User> GetUsers([Service] IUserRepository repo) =>
        repo.GetAllAsync();

    [Authorize(Roles = ["Admin"])]
    public async Task<UserResponseModel> GetUserById([Service] IUserResponseUseCase userResponseUseCase, Guid id) =>
        await userResponseUseCase.ExecuteAsync(id);
}