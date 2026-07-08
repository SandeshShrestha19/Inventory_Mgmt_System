using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;
using HotChocolate.Authorization;

namespace ECommerce.API.GraphQL.Queries;

public class Query
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ProductResponseModel> GetProducts([Service] IProductFacade productFacade, PaginationInput paginationInput) =>
        productFacade.GetAll(paginationInput.CursorId, paginationInput.PageSize);

    public async Task<ProductResponseModel?> GetProductById([Service] IProductFacade productFacade, Guid id, CancellationToken cancellationToken) =>
        await productFacade.GetByIdAsync(id, cancellationToken);

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize(Policy = "ActiveUser")]
    public IQueryable<OrderResponseModel> GetOrders([Service] IOrderFacade orderFacade, PaginationInput paginationInput) =>
        orderFacade.GetAll(paginationInput.CursorId, paginationInput.PageSize);

    [Authorize]
    public async Task<OrderResponseModel?> GetOrderById([Service] IOrderFacade orderFacade, Guid id, CancellationToken cancellationToken) =>
        await orderFacade.GetByIdAsync(id, cancellationToken);

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize(Roles = ["Admin", "Manager"])]
    public IQueryable<UserResponseModel> GetUsers([Service] IUserFacade userFacade, PaginationInput paginationInput) =>
        userFacade.GetAll(paginationInput.CursorId, paginationInput.PageSize);

    [Authorize(Roles = ["Admin"])]
    public async Task<UserResponseModel> GetUserById([Service] IUserFacade userFacade, Guid id, CancellationToken cancellationToken) =>
        await userFacade.GetByIdAsync(id, cancellationToken);

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize(Policy = "ActiveUser")]
    public IQueryable<CategoryResponseModel> GetCategories([Service] ICategoryFacade categoryFacade, PaginationInput paginationInput) =>
        categoryFacade.GetAll(paginationInput.CursorId, paginationInput.PageSize);

    [Authorize(Policy = "ActiveUser")]
    public async Task<CategoryResponseModel> GetUserById([Service] ICategoryFacade categoryFacade, Guid id, CancellationToken cancellationToken) =>
        await categoryFacade.GetByIdAsync(id, cancellationToken);
}