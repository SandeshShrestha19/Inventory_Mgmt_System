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

    public async Task<ProductResponseModel?> GetProductById([Service] IProductFacade productFacade, Guid id) =>
        await productFacade.GetByIdAsync(id);  

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize(Policy = "ActiveUser")]
    public IQueryable<OrderResponseModel> GetOrders([Service] IOrderFacade orderFacade, PaginationInput paginationInput) =>
        orderFacade.GetAll(paginationInput.CursorId, paginationInput.PageSize);

    [Authorize]
    public async Task<OrderResponseModel?> GetOrderById([Service] IOrderFacade orderFacade, Guid id) =>
        await orderFacade.GetByIdAsync(id);

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize(Roles = ["Admin", "Manager"])]
    public IQueryable<UserResponseModel> GetUsers([Service] IUserFacade userFacade, PaginationInput paginationInput) =>
        userFacade.GetAll(paginationInput.CursorId, paginationInput.PageSize);

    [Authorize(Roles = ["Admin"])]
    public async Task<UserResponseModel> GetUserById([Service] IUserFacade userFacade, Guid id) =>
        await userFacade.GetByIdAsync(id);
}