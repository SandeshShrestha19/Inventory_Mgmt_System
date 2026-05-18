namespace ECommerce.Domain.Exceptions;

public class UnauthorizedException : BaseException
{
  public UnauthorizedException(string message = "Invalid email or password"): base(message, "UNAUTHORIZED"){}
}