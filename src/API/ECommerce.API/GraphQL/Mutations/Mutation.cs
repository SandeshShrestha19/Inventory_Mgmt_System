using ECommerce.Domain.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;
using Ecommerce.Domain.Models;
using HotChocolate.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace ECommerce.API.GraphQL.Mutations;

public class Mutation
{
    //Mutation = Controller
    [Authorize(Roles = ["Admin"])]
    public async Task<Product> AddProduct(
        [Service] IProductFacade productFacade,
        ProductInput productInput) =>
        await productFacade.AddAsync(new AddProductModel
        {
            Name = productInput.Name,
            Description = productInput.Description,
            Price = productInput.Price,
            Stock = productInput.Stock
        });

    [Authorize(Roles = ["Admin"])]
    public async Task<bool> UpdateProduct([Service] IProductFacade productFacade, Guid id, UpdateProductInput udpateProductInput)
    {
        await productFacade.UpdateAsync(id, new UpdateProductModel
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
    [Service] IProductFacade productFacade,
    Guid id) =>
    await productFacade.DeleteAsync(id);

    public async Task<User> RegisterUser([Service] IUserFacade userFacade, UserInput userInput)
    {
        return await userFacade.AddAsync(new AddUserModel
        {
            Name = userInput.Name,
            Email = userInput.Email,
            Role = userInput.Role,
            Password = userInput.Password
        });
    }
    [Authorize]
    public async Task<bool> DeleteUser([Service] IUserFacade userFacade, Guid id)
    {
        return await userFacade.DeleteAsync(id);
    }

    [Authorize]
    public async Task<bool> UpdateUser([Service] IUserFacade userFacade, Guid id, UpdateUserInput updateUserInput)
    {
        await userFacade.UpdateAsync(id, new UpdateUserModel
        {
            Name = updateUserInput.Name,
            Email = updateUserInput.Email,
            Role = updateUserInput.Role,
            Password = updateUserInput.Password
        });
        return true;
    }

    [Authorize(Policy = "ActiveUser")]
    public async Task<Order> PlaceOrder(
    [Service] IOrderFacade orderFacade,
    OrderInput orderInput)
    {
        return await orderFacade.AddAsync(new PlaceOrderModel
        {
            UserId = orderInput.UserId,
            Items = orderInput.Items.Select(item => new OrderItemModel
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity
            }).ToList()
        });
    }

    [Authorize(Policy = "ActiveUser")]
    public async Task<bool> DeleteOrder([Service] IOrderFacade orderFacade, Guid id)
    {
        return await orderFacade.DeleteAsync(id);
    }

    [Authorize(Policy = "ActiveUser")]

    public async Task<bool> UpdateOrder(
    [Service] IOrderFacade orderFacade,
    Guid id,
    UpdateOrderInput updateOrderInput)
    {
        await orderFacade.UpdateAsync(id, new UpdateOrderModel
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

        var jti = httpContextAccessor.HttpContext!.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value; //get jti from token

        var expiry = httpContextAccessor.HttpContext!.User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value
        ?? throw new Exception("Token expiry not found!");

        var tokenExpiry = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiry)).UtcDateTime;

        var refreshToken = httpContextAccessor.HttpContext!.Request.Cookies["refreshToken"] ?? throw new Exception("Refresh token not found!"); //request for refresh token from cookie

        await logoutUseCase.ExecuteAsync(Guid.Parse(userId!), refreshToken, jti, tokenExpiry);

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

    [Authorize(Roles = ["Admin"])]
    public async Task<bool> SetUserActiveStatus([Service] ISetUserActiveStatusUseCase setUserActiveStatusUseCase, Guid userId, bool isActive)
    {
        await setUserActiveStatusUseCase.ExecuteAsync(userId, isActive);
        return true;
    }
}