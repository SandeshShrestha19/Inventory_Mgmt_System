namespace ECommerce.Domain.Entities;

public class RefreshToken
{
  public Guid Id {get; set;}
  public string Token {get; set;} = string.Empty;
  public Guid UserId {get; set;}
  public User User {get; set;}
  public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
  public DateTime ExpiresIn {get; set;} 
  public bool IsRevoked {get; set;} = false;
}