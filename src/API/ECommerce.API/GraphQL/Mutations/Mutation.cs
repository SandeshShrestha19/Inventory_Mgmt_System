using ECommerce.Domain.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;
using Ecommerce.Domain.Models;

namespace ECommerce.API.GraphQL.Mutations;

public class Mutation
{
    //Mutation = Controller
    public async Task<Product> AddProduct(
        [Service] IAddProductUseCase addUseCase,
        string name,
        string description,
        decimal price,
        int stock) =>
        await addUseCase.ExecuteAsync(new AddProductModel
        {
            Name = name,
            Description = description,
            Price = price,
            Stock = stock
        });

    public async Task<Product> UpdateProduct([Service] IUpdateProductUseCase updateUseCase, Guid id, string? name, string? description, decimal? price, int? stock)
    {
        return await updateUseCase.ExecuteAsync(id, new UpdateProductModel
        {
            Name = name,
            Description = description,
            Price = price,
            Stock = stock
        });
    }
    public async Task<bool> DeleteProduct(
    [Service] IProductRepository repo,
    Guid id) =>
    await repo.DeleteAsync(id);

    public async Task<User> RegisterUser([Service] IRegisterUserUseCase addUserUseCase, string name, string email, string password)
    {
        return await addUserUseCase.ExecuteAsync(new AddUserModel
        {
            Name = name,
            Email = email,
            Password = password
        });
    }

    public async Task<bool> DeleteUser([Service] IUserRepository repo, Guid id)
    {
        return await repo.DeleteAsync(id);
    }

    public async Task<User> UpdateUser([Service] IUpdateUserUseCase updateUserUseCase, Guid id, string? name, string? email, string? password)
    {
        return await updateUserUseCase.ExecuteAsync(id, new UpdateUserModel
        {
            Name = name,
            Email = email,
            Password = password
        });
    }

    public async Task<Order> PlaceOrder([Service] IPlaceOrderUseCase placeOrderUseCase, Guid userId, List<OrderItemModel> items)
    {
        return await placeOrderUseCase.ExecuteAsync(new PlaceOrderModel
        {
            UserId = userId,
            Items = items
        });
    }

    public async Task<bool> DeleteOrder([Service] IOrderRepository repo, Guid id)
    {
        return await repo.DeleteAsync(id);
    }

    public async Task<Order> UpdateOrder([Service] IUpdateOrderUseCase updateOrderUseCase, Guid id, List<UpdateOrderItemModel> items)
    {
        return await updateOrderUseCase.ExecuteAsync(id, new UpdateOrderModel
        {
            Items = items
        });
    }
}