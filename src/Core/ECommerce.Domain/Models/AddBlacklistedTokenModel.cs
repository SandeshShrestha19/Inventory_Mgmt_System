public class AddBlacklistedTokenModel
{
  public string Jti {get; set;} = string.Empty;
  public DateTime ExpiresAt {get; set;}
}