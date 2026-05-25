namespace ECommerce.Domain.Exceptions;

public class ConflictException : BaseException
{
  public ConflictException(string message) : base(message, "CONFLICT"){}
  public static ConflictException EmailAlreadyExists()
  {
    return new("Email already exists!");
  }
}