namespace ECommerce.Domain.Exceptions;

public class ConflictException : BaseException
{
  public ConflictException(string message) : base(message, "CONFLICT"){}
  public static ConflictException EmailAlreadyExists()
  {
    return new("Email already exists!");
  }
  public static ConflictException ProductNameAlreadyExists(string name)
  {
    return new($"A product with the name '{name}' already exists!");
  }
  public static ConflictException CategoryNameAlreadyExists(string name)
  {
    return new($"A category with the name '{name}' already exists!");
  }
}