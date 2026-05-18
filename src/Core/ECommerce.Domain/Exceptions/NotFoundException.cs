namespace ECommerce.Domain.Exceptions;
public class NotFoundException : BaseException
{
  public NotFoundException(string message) : base(message, "NOT_FOUND")
  {  }
  public static NotFoundException User() => new("User not found!");
  public static NotFoundException Product() => new("Product not found!");
  public static NotFoundException Order() => new("Order not found!");
  public static NotFoundException RefreshToken() => new("Refresh Token not found!");
}