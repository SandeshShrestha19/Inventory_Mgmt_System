namespace ECommerce.Domain.Ports;

public interface IBlacklistedTokenRepository
{
  Task AddAsync(BlacklistedToken blackListedToken);
  Task<bool> IsBlacklistedAsync(string jti);
  Task DeleteExpiredAsync();

}