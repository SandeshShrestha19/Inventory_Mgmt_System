using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;
using HotChocolate.Authorization;

namespace ECommerce.API.GraphQL.Queries;

public class Query
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ProductResponseModel> GetProducts([Service] IGetProductQueryUseCase getProductQueryUseCase, PaginationInput paginationInput) =>
        getProductQueryUseCase.GetAll(paginationInput.CursorId, paginationInput.PageSize);

    public async Task<ProductResponseModel?> GetProductById([Service] IProductResponseUseCase productResponseUseCase, Guid id) =>
        await productResponseUseCase.GetByIdAsync(id);  

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize(Policy = "ActiveUser")]
    public IQueryable<OrderResponseModel> GetOrders([Service] IGetOrderQueryUseCase getOrderQueryUseCase, PaginationInput paginationInput) =>
        getOrderQueryUseCase.GetAll(paginationInput.CursorId, paginationInput.PageSize);

    [Authorize]
    public async Task<OrderResponseModel?> GetOrderById([Service] IOrderResponseUseCase orderResponseUseCase, Guid id) =>
        await orderResponseUseCase.GetByIdAsync(id);

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize(Roles = ["Admin", "Manager"])]
    public IQueryable<UserResponseModel> GetUsers([Service] IGetUserQueryUseCase getUserQueryUseCase, PaginationInput paginationInput) =>
        getUserQueryUseCase.GetAll(paginationInput.CursorId, paginationInput.PageSize);

    [Authorize(Roles = ["Admin"])]
    public async Task<UserResponseModel> GetUserById([Service] IUserResponseUseCase userResponseUseCase, Guid id) =>
        await userResponseUseCase.GetByIdAsync(id);
}