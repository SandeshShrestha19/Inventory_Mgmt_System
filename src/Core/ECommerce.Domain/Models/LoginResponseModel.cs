using HotChocolate;

namespace ECommerce.Domain.Models;

public class LoginResponseModel
{
  public string Email {get; set;} = string.Empty;
  public string Message {get; set;} = string.Empty;
  public int ExpiresIn {get; set;}

  public string AccessToken{get; set;} = string.Empty;
  [GraphQLIgnore]
  public string RefreshToken{get; set;} = string.Empty;
}