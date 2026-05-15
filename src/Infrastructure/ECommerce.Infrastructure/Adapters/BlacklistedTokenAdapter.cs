using ECommerce.Domain.Ports;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class BlacklistedTokenAdapter : IBlacklistedTokenRepository
{
  private readonly AppDbContext _context;
  public BlacklistedTokenAdapter(AppDbContext context)
  {
    _context = context;
  }
  public async Task AddAsync(BlacklistedToken blacklistedToken)
  {
    _context.BlacklistedTokens.Add(blacklistedToken);
    await _context.SaveChangesAsync();
  }

  public async Task DeleteExpiredAsync()
  {
    var blacklistedTokens = await _context.BlacklistedTokens.Where(x => x.ExpiresAt < DateTime.UtcNow).ToListAsync();
    _context.BlacklistedTokens.RemoveRange(blacklistedTokens);
    await _context.SaveChangesAsync();
  }

  public async Task<bool> IsBlacklistedAsync(string jti)
  {
    return await _context.BlacklistedTokens.AnyAsync(x => x.Jti == jti);
  }
}