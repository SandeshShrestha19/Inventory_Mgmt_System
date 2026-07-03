public class BlacklistedToken
{
  public Guid Id { get; set; }
  public string? Jti { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime ExpiresAt { get; set; }
}