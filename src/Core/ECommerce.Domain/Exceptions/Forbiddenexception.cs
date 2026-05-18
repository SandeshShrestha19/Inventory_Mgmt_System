namespace ECommerce.Domain.Exceptions;

public class ForbiddenException : BaseException
{
  public ForbiddenException(string message = "You don't have permission") : base(message, "FORBIDDEN"){}
}