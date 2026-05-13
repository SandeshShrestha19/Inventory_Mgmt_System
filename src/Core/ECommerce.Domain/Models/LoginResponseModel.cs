namespace ECommerce.Domain.Models;

public class LoginResponseModel
{
  public string Email {get; set;} = string.Empty;
  public string Message {get; set;} = string.Empty;
  public int ExpiresIn {get; set;}
}