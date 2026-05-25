namespace ECommerce.Domain.Exceptions;

public class BusinessException : BaseException
{
  public BusinessException(string message) : base(message, "BUSINESS_ERROR"){}

  public static BusinessException AccountDisabled()
  {
    return new("Your account has been disabled! Contact admin.");
  }
  public static BusinessException OutOfStock()
  {
    return new ("Product is out of stock!");
  }
}