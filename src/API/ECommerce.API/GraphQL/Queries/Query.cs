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

    public async Task<Product?> GetProductById([Service] IProductRepository repo, Guid id) =>
        await repo.GetByIdAsync(id);  

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize]
    public IQueryable<Order> GetOrders([Service] IOrderRepository repo) =>
        repo.GetAllAsync();

    [Authorize]
    public async Task<Order?> GetOrderById([Service] IOrderRepository repo, Guid id) =>
        await repo.GetByIdAsync(id);

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize(Roles = ["Admin", "Manager"])]
    public IQueryable<User> GetUsers([Service] IUserRepository repo) =>
        repo.GetAllAsync();

    [Authorize(Roles = ["Admin"])]
    public async Task<User> GetUserById([Service] IUserRepository repo, Guid id) =>
        await repo.GetByIdAsync(id);
}