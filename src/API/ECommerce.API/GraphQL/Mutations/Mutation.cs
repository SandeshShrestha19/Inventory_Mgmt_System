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
        ProductInput productInput) =>
        await addUseCase.ExecuteAsync(new AddProductModel
        {
            Name = productInput.Name,
            Description = productInput.Description,
            Price = productInput.Price,
            Stock = productInput.Stock
        });

    [Authorize(Roles = ["Admin"])]
    public async Task<bool> UpdateProduct([Service] IUpdateProductUseCase updateUseCase, Guid id, UpdateProductInput udpateProductInput)
    {
        await updateUseCase.ExecuteAsync(id, new UpdateProductModel
        {
            Name = udpateProductInput.Name,
            Description = udpateProductInput.Description,
            Price = udpateProductInput.Price,
            Stock = udpateProductInput.Stock
        });
        return true;
    }

    [Authorize(Roles = ["Admin", "Mananger"])]
    public async Task<bool> DeleteProduct(
    [Service] IProductRepository repo,
    Guid id) =>
    await repo.DeleteAsync(id);

    public async Task<User> RegisterUser([Service] IRegisterUserUseCase addUserUseCase, UserInput userInput)
    {
        return await addUserUseCase.ExecuteAsync(new AddUserModel
        {
            Name = userInput.Name,
            Email = userInput.Email,
            Role = userInput.Role,
            Password = userInput.Password
        });
    }
    [Authorize]
    public async Task<bool> DeleteUser([Service] IUserRepository repo, Guid id)
    {
        return await repo.DeleteAsync(id);
    }

    [Authorize]
    public async Task<bool> UpdateUser([Service] IUpdateUserUseCase updateUserUseCase, Guid id, UpdateUserInput updateUserInput)
    {
        await updateUserUseCase.ExecuteAsync(id, new UpdateUserModel
        {
            Name = updateUserInput.Name,
            Email = updateUserInput.Email,
            Role = updateUserInput.Role,
            Password = updateUserInput.Password
        });
        return true;
    }

    [Authorize]
    public async Task<Order> PlaceOrder(
    [Service] IPlaceOrderUseCase placeOrderUseCase,
    OrderInput orderInput)
    {
        return await placeOrderUseCase.ExecuteAsync(new PlaceOrderModel
        {
            UserId = orderInput.UserId,
            Items = orderInput.Items.Select(item => new OrderItemModel
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity
            }).ToList()
        });
    }

    public async Task<bool> DeleteOrder([Service] IOrderRepository repo, Guid id)
    {
        return await repo.DeleteAsync(id);
    }

    [Authorize]

    public async Task<bool> UpdateOrder(
    [Service] IUpdateOrderUseCase updateOrderUseCase,
    Guid id,
    UpdateOrderInput updateOrderInput)
    {
        await updateOrderUseCase.ExecuteAsync(id, new UpdateOrderModel
        {
            Items = updateOrderInput.Items.Select(item => new UpdateOrderItemModel
            {
                ProductId = item.ProductId,  
                Quantity = item.Quantity
            }).ToList() 
        });
        return true;
    }

    [AllowAnonymous]
    public async Task<LoginResponseModel> Login(
    [Service] ILoginUseCase loginUseCase,
    [Service] IHttpContextAccessor httpContextAccessor,
    string email,
    string password)
    {
        var result = await loginUseCase.ExecuteAsync(new LoginModel
        {
            Email = email,
            Password = password
        });

        httpContextAccessor.HttpContext!.Response.Cookies.Append("token", result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(7)
        });

        httpContextAccessor.HttpContext!.Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        return result;
    }


    //clears the cookie
    [Authorize]
    public async Task<bool> Logout([Service] ILogoutUseCase logoutUseCase,
    [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext!.User
        .FindFirst(ClaimTypes.NameIdentifier)?.Value; //get userId from token

        var refreshToken = httpContextAccessor.HttpContext!.Request.Cookies["refreshToken"] ?? throw new Exception("Refresh token not found!"); //request for refresh token from cookie

        await logoutUseCase.ExecuteAsync(Guid.Parse(userId!), refreshToken);

        httpContextAccessor.HttpContext.Response.Cookies.Delete("token");
        httpContextAccessor.HttpContext.Response.Cookies.Delete("refreshToken");

        return true;
    }

    public async Task<string> RefreshToken([Service] IRefreshTokenUseCase refreshTokenUseCase, [Service] IHttpContextAccessor httpContextAccessor, string? refToken = null)
    {
        var refreshToken = httpContextAccessor.HttpContext!.Request.Cookies["refreshToken"] ?? refToken ?? throw new Exception("Refresh token not found!");

        var newAccessToken = await refreshTokenUseCase.ExecuteAsync(refreshToken);

        httpContextAccessor.HttpContext!.Response.Cookies.Append("refreshToken", newAccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(7)
        });
        return newAccessToken;
    }
}