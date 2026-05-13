using ECommerce.Domain.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;
using Ecommerce.Domain.Models;
using HotChocolate.Authorization;
using System.Security.Claims;

namespace ECommerce.API.GraphQL.Mutations;

public class Mutation
{
    //Mutation = Controller
    [Authorize(Roles = ["Admin"])]
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

    [Authorize(Roles = ["Admin"])]
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

    [Authorize(Roles = ["Admin", "Mananger"])]
    public async Task<bool> DeleteProduct(
    [Service] IProductRepository repo,
    Guid id) =>
    await repo.DeleteAsync(id);

    public async Task<User> RegisterUser([Service] IRegisterUserUseCase addUserUseCase, string name, string email, string password, string role)
    {
        return await addUserUseCase.ExecuteAsync(new AddUserModel
        {
            Name = name,
            Email = email,
            Role = role,
            Password = password
        });
    }
    [Authorize]
    public async Task<bool> DeleteUser([Service] IUserRepository repo, Guid id)
    {
        return await repo.DeleteAsync(id);
    }

    [Authorize]
    public async Task<User> UpdateUser([Service] IUpdateUserUseCase updateUserUseCase, Guid id, string? name, string? email, string? password, string? role)
    {
        return await updateUserUseCase.ExecuteAsync(id, new UpdateUserModel
        {
            Name = name,
            Email = email,
            Role = role,
            Password = password,
        });
    }

    [Authorize]
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

    [Authorize]

    public async Task<Order> UpdateOrder([Service] IUpdateOrderUseCase updateOrderUseCase, Guid id, List<UpdateOrderItemModel> items)
    {
        return await updateOrderUseCase.ExecuteAsync(id, new UpdateOrderModel
        {
            Items = items
        });
    }

    [AllowAnonymous]
    public async Task<LoginResponseModel> Login(
    [Service] ILoginUseCase loginUseCase,
    [Service] IHttpContextAccessor httpContextAccessor,
    string email,
    string password)
    {
        var token = await loginUseCase.ExecuteAsync(new LoginModel
        {
            Email = email,
            Password = password
        });

        httpContextAccessor.HttpContext!.Response.Cookies.Append("token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict, 
            Expires = DateTimeOffset.UtcNow.AddMinutes(7)
        });

        return new LoginResponseModel
        {
            Email = email,
            Message = "Login successful!",
            ExpiresIn = 7
        };
    }


    //clears the cookie
    [Authorize]
    public async Task<bool> Logout([Service] ILogoutUseCase logoutUseCase,
    [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext!.User
        .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await logoutUseCase.ExecuteAsync(Guid.Parse(userId!));

        httpContextAccessor.HttpContext.Response.Cookies.Delete("token");

        return true;
    }
}