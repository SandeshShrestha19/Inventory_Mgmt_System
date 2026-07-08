using ECommerce.Domain.Entities;

public class ResponseMapper
{
  public static ProductResponseModel ToProductResponse(Product product)
  {
    return new ProductResponseModel
    {
      Id = product.Id,
      Name = product.Name,
      Stock = product.Stock,
      Price = product.Price,
      CreatedAt = product.CreatedAt,
      Description = product.Description,
      CategoryId = product.CategoryId,
      ProductImages = product.ProductImages
    };
  }

  public static UserResponseModel ToUserResponse(User user)
  {
    return new UserResponseModel
    {
      Id = user.Id,
      Email = user.Email,
      Orders = user.Orders,
      CreatedAt = user.CreatedAt,
      Role = user.Role,
      IsLoggedIn = user.IsLoggedIn,
      Name = user.Name
    };
  }

  public static OrderResponseModel ToOrderResponse(Order order)
  {
    return new OrderResponseModel
    {
      Id = order.Id,
      TotalPrice = order.TotalPrice,
      OrderItems = order.OrderItems,
      OrderDate = order.OrderDate
    };
  }

  public static CategoryResponseModel ToCategoryResponse(Category category)
  {
    return new CategoryResponseModel
    {
      Id = category.Id,
      Name = category.Name,
      Products = category.Products,
      CreatedAt = category.CreatedAt
    };
  }
}